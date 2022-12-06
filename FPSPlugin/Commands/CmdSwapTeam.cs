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

using FPS.Configuration;
using FPS.Entities;
using FPS.Teams;
using MCGalaxy;
using System;
using System.Collections.Generic;
using System.IO;

namespace FPS.Commands;

internal class CmdSwapTeam : Command2
{
    public override string name { get { return "FPSMOSwapTeam"; } }
    public override string shortcut { get { return "SwapTeam"; } }
    public override string type { get { return CommandTypes.Games; } }
    public override bool SuperUseable { get { return false; } }

    private readonly TimeSpan _spanBetweenSwaps = TimeSpan.FromSeconds(10);

    public override void Use(Player p, string message)
    {
        FPSGame game = FPSGame.Instance;

        if (!game.IsInGame(p))
        {
            p.Message("&WYou can only change team when playing.");
            return;
        }

        if (!game.IsTeamSwappingAllowed)
        {
            p.Message("&WCannot swap team while voting.");
            return;
        }

        int timeRemainingSeconds = 0;
        if (SwappedTooRecently(p, ref timeRemainingSeconds))
        {
            p.Message($"Need to wait another {timeRemainingSeconds} seconds before swapping again");
            return;
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
        PlayerDataHandler.Instance.dictPlayerData[p.truename].lastTeamSwap = DateTime.Now;
    }

    public override void Help(Player p)
    {
        p.Message("&T/SwapTeam");
        p.Message("&HSwaps team");
    }

    private bool SwappedTooRecently(Player player, ref int timeRemainingSeconds)
    {
        Dictionary<string, PlayerData> dictPlayerData = PlayerDataHandler.Instance.dictPlayerData;

        if (!dictPlayerData.ContainsKey(player.truename))
        {
            throw new ArgumentException($"There is no player {player.truename} in {nameof(dictPlayerData)}.",
                nameof(player));
        }

        PlayerData playerData = dictPlayerData[player.truename];

        DateTime now = DateTime.Now;
        TimeSpan elapsedSinceLastUse = now - playerData.lastTeamSwap;
        timeRemainingSeconds = (int)(_spanBetweenSwaps - elapsedSinceLastUse).TotalSeconds;

        return (timeRemainingSeconds >= 0);

    }
}
