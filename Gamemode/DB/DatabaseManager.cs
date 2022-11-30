using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCGalaxy;
using FPSMO.Entities;
using MCGalaxy.SQL;
using System.Data;
using FPSMO.Configuration;

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

        internal string[] GetMapPool()
        {
            var mapPool = new List<string>();
            List<string[]> matches = Database.GetRows("FPS_MapPool", "map_name");

            foreach (string[] match in matches)
            {
                mapPool.Add(match[0]);
            }

            return mapPool.ToArray();
        }

        internal bool IsInMapPool(string map)
        {
            string[] mapPool = GetMapPool();
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
    }
}
