using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCGalaxy;
using FPSMO.Entities;
using MCGalaxy.SQL;
using System.Data;
using FPSMO.Configuration;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using MCGalaxy.DB;
using MCGalaxy.Events.LevelEvents;

namespace FPSMO.DB
{
    internal class DatabaseManager
    {
        internal void Observe(AchievementsManager manager)
        {
            manager.AchievementUnlocked += HandleAchievementUnlocked;
        }

        internal void Unobserve(AchievementsManager manager)
        {
            manager.AchievementUnlocked -= HandleAchievementUnlocked;
        }

        internal void ObserveMCGalaxy()
        {
            OnLevelDeletedEvent.Register(HandleLevelDeleted, Priority.Normal);
            OnLevelRenamedEvent.Register(HandleLevelRenamed, Priority.Normal);
        }

        internal void UnobserveMCGalaxy()
        {
            OnLevelDeletedEvent.Unregister(HandleLevelDeleted);
            OnLevelRenamedEvent.Unregister(HandleLevelRenamed);
        }

        internal void CreateTables(bool checkExistence)
        {
            foreach (string tableName in Query.Create.Keys)
            {
                if (!checkExistence || !Database.TableExists(tableName))
                {
                    Database.Execute(Query.Create[tableName]);
                }
            }
        }

        private void HandleAchievementUnlocked(Object sender, AchievementUnlockedEventArgs args)
        {
            Database.AddRow("FPS_PlayerAchievement",
                "Player, AchievementName",
                args.Player.truename, args.Achievement.Name);
        }

        internal MapData GetMapData(string mapName)
        {
            List<string[]> matches = Database.GetRows("FPS_MapData", "name,countdown_duration_seconds,round_duration_seconds",
                                                      $"WHERE name=@0", mapName);

            if (matches.Count == 0) return null;
            string[] entry = matches[0];

            MapData mapData = new MapData();
            mapData.Name = entry[0];

            if (entry[1] == "") mapData.CountdownTimeSeconds = null;
            else mapData.CountdownTimeSeconds = uint.Parse(entry[1]);

            if (entry[2] == "") mapData.RoundDurationSeconds = null;
            else mapData.RoundDurationSeconds = uint.Parse(entry[2]);

            return mapData;
        }

        internal void RemoveMapData(string mapName)
        {
            Database.DeleteRows("FPS_MapData", "WHERE name=@0", mapName);
        }

        internal void SetMapCountdownDuration(string mapName, int countdownDuration)
        {
            if (HasMapData(mapName))
            {
                Database.UpdateRows("FPS_MapData", "countdown_duration_seconds=@1", "WHERE name=@0", mapName, countdownDuration);
            }
            else
            {
                Database.AddRow("FPS_MapData", "name, countdown_duration_seconds", mapName, countdownDuration);
            }
        }

        internal void SetMapRoundDuration(string mapName, int roundDuration)
        {
            if (HasMapData(mapName))
            {
                Database.UpdateRows("FPS_MapData", "round_duration_seconds=@1", "WHERE name=@0", mapName, roundDuration);
            }
            else
            {
                Database.AddRow("FPS_MapData", "name, round_duration_seconds", mapName, roundDuration);
            }
        }

        private bool HasMapData(string mapName)
        {
            return (GetMapData(mapName) != null);
        }

        internal string[] GetMapsPool()
        {
            var mapPool = new List<string>();
            List<string[]> matches = Database.GetRows("FPS_MapPool", "map_name");

            foreach (string[] match in matches)
            {
                mapPool.Add(match[0]);
            }

            return mapPool.ToArray();
        }

        internal bool IsInMapsPool(string map)
        {
            string[] mapPool = GetMapsPool();
            return (mapPool.Contains(map));
        }

        internal void AddMap(string map)
        {
            Database.AddRow("FPS_MapPool", "map_name", map);
        }

        internal void RemoveMap(string map)
        {
            Database.DeleteRows("FPS_MapPool", "WHERE map_name=@0", map);
        }

        internal int? GetRating(string mapName, Player player)
        {
            List<string[]> matches = Database.GetRows("FPS_Rating", "rating",
                                                      $"WHERE map_name=@0 and player_name=@1", mapName, player.truename);

            if (matches.Count == 0)
            {
                return null;
            }
            else
            {
                string[] onlyMatch = matches[0];
                return int.Parse(onlyMatch[0]);
            }
        }

        internal void SetRating(string mapName, Player player, int rating)
        {
            int? previousRating = GetRating(mapName, player);

            if (previousRating is null)
            {
                Database.AddRow("FPS_Rating", "map_name, player_name, rating", mapName, player.truename, rating);
            }
            else if (previousRating != rating)
            {
                Database.UpdateRows("FPS_Rating", "rating=@2", "WHERE map_name=@0 and player_name=@1",
                                     mapName, player.truename, rating);
            }
        }

        internal void RemoveRating(string mapName, Player player)
        {
            Database.DeleteRows("FPS_Rating", "WHERE map_name=@0 and player_name=@1", mapName, player.truename);
        }

        internal float? AverageRating(string mapName)
        {
            List<string[]> matches = Database.GetRows("FPS_Rating",
                "AVG(rating)", $"WHERE map_name=@0 GROUP BY map_name", mapName);

            if (matches.Count == 0)
            {
                return null;
            }
            else
            {
                string[] onlyMatch = matches[0];
                float result = float.Parse(onlyMatch[0], System.Globalization.CultureInfo.InvariantCulture);
                return result;
            }
        }

        internal void HandleLevelDeleted(string mapName)
        {
            Database.UpdateRows("FPS_Player", "favourite_map_name=NULL", "WHERE favourite_map_name=@0", mapName);
            Database.DeleteRows("FPS_MapPool", "WHERE map_name=@0", mapName);
            Database.DeleteRows("FPS_Rating", "WHERE map_name=@0", mapName);
            Database.DeleteRows("FPS_SpawnPoint", "WHERE map_name=@0", mapName);
            Database.DeleteRows("FPS_MapData", "WHERE name=@0", mapName);
        }

        internal void HandleLevelRenamed(string previousName, string newName)
        {
            Database.UpdateRows("FPS_Player", "favourite_map_name=@0", "WHERE favourite_map_name=@1",
                newName, previousName);
            Database.UpdateRows("FPS_Rating", "map_name=@0", "WHERE map_name=@1",
                newName, previousName);
            Database.UpdateRows("FPS_SpawnPoint", "map_name=@0", "WHERE map_name=@1",
                newName, previousName);
            Database.UpdateRows("FPS_Round", "map_name=@0", "WHERE map_name=@1",
                newName, previousName);
            Database.UpdateRows("FPS_MapPool", "map_name=@0", "WHERE map_name=@1",
                newName, previousName);
            Database.UpdateRows("FPS_MapData", "name=@0", "WHERE name=@1",
                newName, previousName);
        }
    }
}
