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
using System.Collections.Generic;

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
                        config.maps.Add(p.level.name);
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
                    break;
            }
            // TODO: Implement this command
        }

        public override void Help(Player p, string message)
        {
            string[] args = message.ToLower().SplitSpaces();
            if (args[0] == "set")
            {
                p.Message("&T/FPSMO set roundtime [time] - Sets the roundtime for this map");
                p.Message("&T/FPSMO set buildable [modify/nomodify] - Sets the build permissions for this map");
            } else
            {
                Help(p);
            }
        }

        public override void Help(Player p)
        {
            p.Message("&");
            p.Message("&HManages or starts the FPSMO gamemode");
        }
    }
}
