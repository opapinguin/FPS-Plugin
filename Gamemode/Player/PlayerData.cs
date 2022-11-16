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
using MCGalaxy.DB;
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static FPSMO.FPSMOGame;

namespace FPSMO.Entities
{
    /// <summary>
    /// Round-specific data i.e., data that changes regularly throughout the round
    /// </summary>
    internal class PlayerData
    {
        internal PlayerData(Player p)
        {
            // Initialize weapons
            gun = new GunWeapon(p);
            rocket = new RocketWeapon(p);
            ResetData();
            name = p.truename;
        }

        internal string name;

        internal ushort hitsGiven;
        internal ushort hitsReceived;
        internal ushort kills;
        internal ushort deaths;

        internal ushort stamina;
        internal ushort health;
        internal string team;

        internal bool bVoted;
        internal ushort vote; // Can be 1 2 or 3

        // Weapons
        internal Weapon currentWeapon;

        internal GunWeapon gun;
        internal RocketWeapon rocket;

        internal DateTime lastWeaponSpeedChange;
        internal DateTime lastTeamSwap;
        internal DateTime lastHealthChange;

        internal uint lastHealth;

        internal void ResetData()
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

        internal static PlayerDataHandler Instance
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
        internal Dictionary<string, PlayerData> dictPlayerData = new Dictionary<string, PlayerData>();
        internal int numPlayers = 0;

        /******************
         * HELPER METHODS *
         ******************/
        internal PlayerData this[string name] // Shame we can't have a static class implement this, would be nicer than using the singleton pattern
        {
            get { PlayerData val; return dictPlayerData.TryGetValue(name, out val) ? val : null; }
            set {
                dictPlayerData[name] = value;
                numPlayers = dictPlayerData.Values.Count();
            }
        }

        internal bool PlayerExists(string name)
        {
            return dictPlayerData.ContainsKey(name);
        }

        internal void ResetPlayerData()
        {            
            foreach (string key in dictPlayerData.Keys)
            {
                dictPlayerData[key].ResetData();
            }
        }

        internal void Deactivate()
        {
            dictPlayerData = new Dictionary<string, PlayerData>();
            numPlayers = 0;
        }
    }
}
