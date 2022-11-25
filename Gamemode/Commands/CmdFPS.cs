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
    class CmdFPS : Command2
    {
        public override string name { get { return "fps"; } }
        public override string shortcut { get { return "q"; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override string type { get { return CommandTypes.Games; } }

        private FPSMOGame _game;

        internal CmdFPS(FPSMOGame game)
        {
            this._game = game;
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
                default:
                    Help(p);
                    return;
            }
        }

        public override void Help(Player player)
        {
            player.Message("&T/fps start [map] &H- Starts First-person shooter.");
            player.Message("&T/fps stop &H- Stops First-person shooter.");
            player.Message("&T/fps end &H- Ends current round of First-person shooter.");
            player.Message("&T/fps add [map] &H- Adds map to the map pool.");
            player.Message("&T/fps remove [map] &H- Removes map from the map pool.");
        }

        private void Start(Player player, string[] args)
        {
            throw new NotImplementedException();
        }

        private void Stop(Player player, string[] args)
        {
            throw new NotImplementedException();
        }

        private void End(Player player, string[] args)
        {
            throw new NotImplementedException();
        }

        private void Add(Player player, string[] args)
        {
            throw new NotImplementedException();
        }

        private void Remove(Player player, string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
