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

using FPS.Entities;
using MCGalaxy;
using System;
using System.Collections.Generic;
using System.Data;

namespace FPS
{
    internal class Team
    {
        internal ushort totalKills;
        internal ushort totalDeaths;
        internal string name;
        internal Dictionary<string, Player> players;
        internal int Count { get { return players.Count; } }

        internal Team(string name)
        {
            this.totalKills = 0;
            this.totalDeaths = 0;
            this.name = name.ToUpper();
            this.players = new Dictionary<string, Player>();
        }

        internal void Add(Player p)
        {
            PlayerData pData = PlayerDataHandler.Instance[p.truename];
            if (pData != null) {
                pData.team = name;
                PlayerDataHandler.Instance[p.truename] = pData;
            }

            players[p.truename] = p;
        }

        internal void Remove(Player p)
        {
            PlayerData pData = PlayerDataHandler.Instance[p.truename];
            if (pData != null) {
                pData.team = "NONE";
                PlayerDataHandler.Instance[p.truename] = pData;
            }

            if (players.ContainsKey(p.truename)) { players.Remove(p.truename); }
        }

        internal bool Contains(Player p)
        {
            return players.ContainsKey(p.truename);
        }

        internal void Reset()
        {
            Dictionary<string, Player> playersCopy = new Dictionary<string, Player>(players);

            foreach (Player p in playersCopy.Values)
            {
                Remove(p);
            }

            players = new Dictionary<string, Player>();
            totalKills = 0;
            totalDeaths = 0;
        }
    }
}

