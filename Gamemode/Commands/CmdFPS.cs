/*
Copyright 2022 WOCC Team

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge,
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using FPSMO.Configuration;
using FPSMO.DB;
using MCGalaxy;
using MCGalaxy.Commands;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FPSMO.Commands
{
    class CmdFPS : Command2
    {
        public override string name { get { return "fps"; } }
        public override string shortcut { get { return "q"; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override string type { get { return CommandTypes.Games; } }

        private FPSMOGame _game;
        private DatabaseManager _databaseManager;

        internal CmdFPS(FPSMOGame game, DatabaseManager databaseManager)
        {
            _game = game;
            _databaseManager = databaseManager;
        }

        public override void Use(Player p, string message)
        {
            string[] args = message.ToLower().SplitSpaces();

            switch (args[0])
            {
                case "start":
                    Start(p, args);
                    break;
                case "stop":
                    Stop(p, args);
                    break;
                case "end":
                    End(p, args);
                    break;
                case "add":
                    Add(p, args);
                    break;
                case "remove":
                    Remove(p, args);
                    break;
                case "config":
                    Config(p, args);
                    break;
                default:
                    Help(p);
                    return;
            }
        }

        public override void Help(Player player, string message)
        {
            if (message == "config")
            {
                HelpConfig(player);
            }
            else
            {
                Help(player);
            }
        }

        public override void Help(Player player)
        {
            player.Message("&T/fps start [map] &H- Starts First-person shooter.");
            player.Message("&T/fps stop &H- Stops First-person shooter.");
            player.Message("&T/fps end &H- Ends current round of First-person shooter.");
            player.Message("&T/fps add [map] &H- Adds map to the map pool.");
            player.Message("&T/fps remove [map] &H- Removes map from the map pool.");
            player.Message("&T/fps config [map]&H- Manages configuration for &T<map>&H.");
            player.Message("&HRun &T/help fps config &Hfor help about map configuration.");
        }

        private void HelpConfig(Player player)
        {
            player.Message("&T/fps config <map> info &H- Prints config for &T<map>&H.");
            player.Message("&T/fps config <map> set <property> <value> &H- Sets config.");
            HelpAvailableProperties(player);
            player.Message("&T/fps config <map> erase &H- Erases config for &T[map]&H.");
        }

        private void HelpAvailableProperties(Player player)
        {
            player.Message("Available properties:");
            player.Message("&H+ &Tcountdown_duration &H- in seconds.");
            player.Message("&H+ &Tround_duration &H- in seconds.");
        }

        private void Start(Player player, string[] args)
        {
            if (_game.bRunning)
            {
                player.Message("&WFPS is already running.");
                return;
            }

            string map = null;

            if (args.Length >= 2)
            {
                map = args[1];
            }
            else if (!player.IsSuper && _databaseManager.IsInMapPool(player.Level.name))
            {
                map = player.Level.name;
            }

            string[] mapPool = _databaseManager.GetMapPool();

            if (map is null)
            {
                if (mapPool.Length == 0)
                {
                    player.Message("&WCould not start FPS: the map pool is empty.");
                    player.Message("&WAdd some maps with &T/fps add [map] &Wfirst.");
                    return;
                }

                _game.Start();
            }
            else
            {
                if (!mapPool.CaselessContains(map))
                {
                    player.Message($"&WCould not start FPS.");
                    player.Message($"&WThere is no map &T{map} &Win the map pool.");
                    player.Message($"&WAdd it first by running &T/fps add {map}&W.");
                    return;
                }

                _game.Start(map);
            }
        }

        private void Stop(Player player, string[] args)
        {
            if (!_game.bRunning)
            {
                player.Message("&WThere is no FPS game running.");
                return;
            }

            _game.Stop();
        }

        private void End(Player player, string[] args)
        {
            player.Message("&WThis command wasn't implemented yet.");
        }

        private void Add(Player player, string[] args)
        {
            string map;

            if (args.Length >= 2)
            {
                map = args[1];
            }
            else if (!player.IsSuper)
            {
                map = player.Level.name;
            }
            else
            {
                player.Message("&WPlease provide a map to add to the map pool.");
                return;
            }

            if (map == Server.Config.MainLevel)
            {
                player.Message("&WMain level cannot be added added to the map pool.");
                return;
            }

            if (_databaseManager.IsInMapPool(map))
            {
                player.Message($"&WMap &T{map} &Wis already in map pool.");
                return;
            }

            if (!LevelInfo.MapExists(map))
            {
                player.Message($"&WMap &T{map} &Wdoes not exist.");
            }

            _databaseManager.AddMap(map);
            player.Message($"&SMap &T{map} &Swas added to the pool.");
        }

        private void Remove(Player player, string[] args)
        {
            string map;

            if (args.Length >= 2)
            {
                map = args[1];
            }
            else if (!player.IsSuper)
            {
                map = player.Level.name;
            }
            else
            {
                player.Message("&WPlease provide a map to remove from the map pool.");
                return;
            }

            if (!_databaseManager.IsInMapPool(map))
            {
                player.Message($"&WMap &T{map} &Wis not is the map pool.");
                return;
            }

            if (_game.bRunning && (_game.map.name == map))
            {
                player.Message($"&WCannot remove &T{map}&W: it's being played.");
                return;
            }

            _databaseManager.RemoveMap(map);
            player.Message($"&SMap &T{map} &Swas removed from the pool.");
        }

        private void Config(Player player, string[] args)
        {
            if (args.Length <= 2)
            {
                HelpConfig(player);
                return;
            }

            string map = args[1];

            switch (args[2])
            {
                case "info":
                    ConfigurationMapInfo(player, map);
                    break;
                case "set":
                    ConfigurationMapSet(player, map, args);
                    break;
                case "erase":
                    ConfigurationMapErase(player, map);
                    break;
                default:
                    HelpConfig(player);
                    break;
            }
        }

        private void ConfigurationMapErase(Player player, string map)
        {
            if (_databaseManager.GetMapData(map) is null)
            {
                player.Message($"&WMap &T{map} &Whasn't been configured. Nothing to erase.");
                return;
            }

            if (_databaseManager.IsInMapPool(map))
            {
                player.Message($"&WCannot erase configuration for &T{map}&W: it's in the map pool.");
                player.Message($"&WPlease run &T/fps remove {map} &Wfirst.");
                return;
            }

            _databaseManager.RemoveMapData(map);
            player.Message($"&SConfiguration was erased for &T{map}&S.");
        }

        private void ConfigurationMapSet(Player player, string map, string[] args)
        {
            if (!LevelInfo.MapExists(map))
            {
                player.Message($"&WThere is no map called &T{map}&W.");
                return;
            }

            if (args.Length != 5)
            {
                HelpConfig(player);
                return;
            }

            string property = args[3];
            string value = args[4];

            switch (property.ToLower())
            {
                case "countdown_duration":
                    int countdownDurationSeconds = 0;

                    if (CommandParser.GetInt(player, value, "countdown_duration",
                                             ref countdownDurationSeconds, min: 0))
                    {
                        _databaseManager.SetMapCountdownDuration(map, countdownDurationSeconds);
                        player.Message("&Tcountdown_duration &Shas been set to " +
                                      $"&T{countdownDurationSeconds}&Ss for &T{map}&S.");
                    }

                    break;
                case "round_duration":
                    int roundDurationSeconds = 0;

                    if (CommandParser.GetInt(player, value, "round_duration",
                                             ref roundDurationSeconds, min: 0))
                    {
                        _databaseManager.SetMapRoundDuration(map, roundDurationSeconds);
                        player.Message("&Tround_duration &Shas been set to " +
                                      $"&T{roundDurationSeconds}&Ss for &T{map}&S.");
                    }


                    break;
                default:
                    HelpAvailableProperties(player);
                    break;
            }
        }

        private void ConfigurationMapInfo(Player player, string map)
        {
            MapData mapData = _databaseManager.GetMapData(map);

            if (mapData is null)
            {
                player.Message($"&WMap &T{map} &Hhasn't been configured yet.");
                player.Message($"&WConfigure it by setting any property.");
                return;
            }

            player.Message($"&SFPS configuration for &T{map}&S.");

            if (mapData.CountdownTimeSeconds is null)
                player.Message("&S+ &Tcountdown_duration &Shasn't been set yet.");
            else
                player.Message($"&S+ &Tcountdown_duration&S: &T{mapData.CountdownTimeSeconds}s&S.");

            if (mapData.RoundDurationSeconds is null)
                player.Message("&S+ &Tround_duration &Shasn't been set yet.");
            else
                player.Message($"&S+ &Tround_duration&S: &T{mapData.RoundDurationSeconds}s&S.");
        }
    }
}