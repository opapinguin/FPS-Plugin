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

using FPSMO.Weapons;
using MCGalaxy;
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPSMO.Entities
{
    /// <summary>
    /// Round-specific data i.e., data that changes regularly throughout the round
    /// </summary>
    internal class PlayerData
    {
        public PlayerData(Player p)
        {
            // Initialize weapons
            gun = new GunWeapon(p);
            rocket = new RocketWeapon(p);
            ResetData();
            name = p.truename;
        }

        public string name;

        public ushort hitsGiven;
        public ushort hitsReceived;
        public ushort kills;
        public ushort deaths;

        public ushort stamina;
        public ushort health;
        public string team;

        public bool bVoted;
        public ushort vote; // Can be 1 2 or 3

        // Weapons
        public Weapon currentWeapon;

        public GunWeapon gun;
        public RocketWeapon rocket;

        // The below fields help us prevent sending the same message twice. This keeps ping low/prevents the packet queue from clogging up
        public string lastCPEStatus1, lastCPEStatus2, lastCPEStatus3,
            lastCPEBottomRight1, lastCPEBottomRight2, lastCPEBottomRight3,
            lastCPEAnnouncement, lastCPESmallAnnouncement, lastCPEBigAnnouncement;

        public DateTime lastWeaponSpeedChange;
        public DateTime lastTeamSwap;

        public void ResetData()
        {
            hitsGiven = kills = deaths = 0;
            stamina = health = 10;
            bVoted = false;
            gun.Reset();
            rocket.Reset();
            currentWeapon = gun;
        }
    }

    /// <summary>
    /// Handles all player data across the game
    /// </summary>
    internal class PlayerDataHandler
    {
        /*************************
        * SINGLETON BOILERPLATE *
        *************************/
        #region Singleton Boilerplate
        private static PlayerDataHandler instance = new PlayerDataHandler();
        private static readonly object padlock = new object();

        public static PlayerDataHandler Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new PlayerDataHandler();
                    }
                    return instance;
                }
            }
        }

        #endregion
        
        /**********
         * FIELDS *
         **********/
        public Dictionary<string, PlayerData> dictPlayerData = new Dictionary<string, PlayerData>();
        public int numPlayers = 0;

        /******************
         * HELPER METHODS *
         ******************/
        public PlayerData this[string name] // Shame we can't have a static class implement this, would be nicer than using the singleton pattern
        {
            get { PlayerData val; return dictPlayerData.TryGetValue(name, out val) ? val : null; }
            set {
                dictPlayerData[name] = value;
                numPlayers = dictPlayerData.Values.Count();
            }
        }

        public bool PlayerExists(string name)
        {
            return dictPlayerData.ContainsKey(name);
        }

        public void ResetPlayerData()
        {            
            foreach (string key in dictPlayerData.Keys)
            {
                dictPlayerData[key].ResetData();
            }
        }

        public void Deactivate()
        {
            dictPlayerData = new Dictionary<string, PlayerData>();
            numPlayers = 0;
        }
    }
}
