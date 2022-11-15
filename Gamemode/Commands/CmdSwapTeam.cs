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
using FPSMO.Entities;
using FPSMO.Teams;
using MCGalaxy;
using System;
using System.IO;

namespace FPSMO.Commands
{
    internal class CmdSwapTeam : Command2
    {
        public override string name { get { return "FPSMOSwapTeam"; } }
        public override string shortcut { get { return "SwapTeam"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data)
        {
            // Check if player is registered in the game to start with
            if (FPSMOGame.Instance.players.ContainsKey(p.truename)) return;

            // Check if round or countdown is actually in progress
            if (FPSMOGame.Instance.stage == FPSMOGame.Stage.Voting) return;

            // Check if round time close to end
            int secondsToRoundsEnd = (int)(FPSMOGame.Instance.RoundEnd - DateTime.Now).TotalSeconds;
            if (secondsToRoundsEnd < 30)
            {
                p.Message("Cannot swap team in the last 30 seconds of the round");
            }

            // Check if not swapped too recently
            int secondsSinceLastSwap = (int)(DateTime.Now - PlayerDataHandler.Instance.dictPlayerData[p.truename].lastTeamSwap).TotalSeconds;
            if (secondsSinceLastSwap < 10)
            {
                p.Message(String.Format("Need to wait another {0} seconds before swapping again", 10 - secondsSinceLastSwap)); return;
            }
            
            // Check if team is not going empty after swap
            if (TeamHandler.GetTeam(p).Count <= 1)
            {
                p.Message(String.Format("Cannot swap as your team only has one player in it")); return;
            }

            // Swap teams
            if (TeamHandler.blue.Contains(p))
            {
                TeamHandler.blue.Remove(p);
                TeamHandler.red.Add(p);
            } else if (TeamHandler.red.Contains(p))
            {
                TeamHandler.red.Remove(p);
                TeamHandler.blue.Add(p);
            }
        }

        public override void Help(Player p)
        {
            p.Message("&T/SwapTeam");
            p.Message("&HSwaps team");
        }
    }
}
