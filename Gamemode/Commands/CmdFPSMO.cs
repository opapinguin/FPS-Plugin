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
using MCGalaxy;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FPSMO.Commands
{
    class CmdFPSMO : Command2
    {
        public override string name { get { return "FPSMO"; } }
        public override string shortcut { get { return "q"; } }

        public override string type { get { return CommandTypes.Games; } }

        public override void Use(Player p, string message)
        {
            string[] args = message.ToLower().SplitSpaces();

            switch (args.Length)
            {
                case 1:
                    if (args[0] == "")
                    {
                        Help(p);
                        return;
                    } else if (args[0] == "add")
                    {
                        // Create config if not exists
                        FPSMOConfig<FPSMOMapConfig>.Create(p.level.name, new FPSMOMapConfig());

                        // Read and write the game config
                        FPSMOGameConfig config = FPSMOConfig<FPSMOGameConfig>.Read("Config");
                        config.MAPS.Add(p.level.name);
                        FPSMOConfig<FPSMOGameConfig>.Update("Config", config);

                        // Note: changes won't occur in-game till next round

                        p.Message("Added this map to the list of FPSMO levels");
                        return;
                    } else if (args[0] == "start")
                    {
                        FPSMOGame.Instance.Start(p.level.name);
                        return;
                    } else if (args[0] == "stop")
                    {
                        FPSMOGame.Instance.Stop();
                        return;
                    }
                    break;
                case 3:
                    if (args[0] == "game")
                    {
                        FPSMOGameConfig config = FPSMOConfig<FPSMOGameConfig>.Read("Config");
                        p.Message(config.MS_UPDATE_ROUND_STATUS.ToString());

                        Type type = typeof(FPSMOGameConfig);
                        PropertyInfo prop = type.GetProperty(args[1].ToUpper());
                        if (prop == null) { p.Message("Not a valid property"); return; }

                        var propVal = prop.GetValue(config, null);
                        object newVal = TryConvert(args[2], propVal);

                        if (newVal == null) { p.Message("Input could not be cast to the property"); return; }

                        prop.SetValue(config, newVal, null);

                        FPSMOConfig<FPSMOGameConfig>.Update("Config", config);
                    } else if (args[0] == "set" && args[1] == "map")
                    {

                    }
                    break;
            }
            // TODO: Implement this command


        }

        /// <summary>
        /// Tries to convert a string into the type of object val. Returns null if not possible.
        /// </summary>
        private static object TryConvert(string str, object val) {
            try
            {
                return Convert.ChangeType(str, val.GetType());
            } catch {
                return null;
            }
        }

        public override void Help(Player p, string message)
        {
            string[] args = message.ToLower().SplitSpaces();
            if (args[0] == "set")
            {
                p.Message("&T/FPSMO map [property] [value] - Sets a property for a map");
                p.Message("&T/FPSMO game [property] [value] - Sets a property for the game as a whole");
                p.Message("&HSee '/help FPSMO map properties' and '/help FPSMO game properties' for a list of properties");
                p.Message("&HUse '/FPSMO get map' and '/help get game' for a list of current properties");
            }
            else
            {
                Help(p);
            }
        }

        public override void Help(Player p)
        {
            p.Message("&HManages the FPSMO gamemode");
            p.Message("&HSee /help FPSMO set for more information");
        }
    }
}
