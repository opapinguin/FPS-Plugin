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

using FPSMO.Entities;
using FPSMO.Weapons;
using MCGalaxy;
using System;
using System.Linq;

namespace FPSMO
{
    /// <summary>
    /// This file contains helper methods in charge of the GUI, for instance player stamina
    /// </summary>
    public sealed partial class FPSMOGame
    {
        /*******************
         * GLOBAL MESSAGES * 
         *******************/
        #region Global Messages

        public delegate void Message(Player p);
        /// <summary>
        // Send a message to everyone on the map, e.g. ShowMap(ShowLevel) based on another function
        /// </summary>
        public void ShowToAll(Message fm)
        {
            foreach (Player p in players.Values)
            {
                fm(p);
            }
        }

        /// <summary>
        /// Sends a CPE message to a player, but does not send if the last CPEMessage was the same one
        /// </summary>
        public void SendCpeMessageNoRepeat(Player p, CpeMessageType type, string message)
        {
            switch (type)
            {
                case CpeMessageType.Announcement:
                    if (!(PlayerDataHandler.Instance[p.truename].lastCPEAnnouncement == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.truename].lastCPEAnnouncement = message; } break;
                case CpeMessageType.SmallAnnouncement:
                    if (!(PlayerDataHandler.Instance[p.truename].lastCPESmallAnnouncement == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.truename].lastCPESmallAnnouncement = message; } break;
                case CpeMessageType.BigAnnouncement:
                    if (!(PlayerDataHandler.Instance[p.truename].lastCPEBigAnnouncement == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.truename].lastCPEBigAnnouncement = message; } break;
                case CpeMessageType.BottomRight1:
                    if (!(PlayerDataHandler.Instance[p.truename].lastCPEBottomRight1 == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.truename].lastCPEBottomRight1 = message; } break;
                case CpeMessageType.BottomRight2:
                    if (!(PlayerDataHandler.Instance[p.truename].lastCPEBottomRight2 == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.truename].lastCPEBottomRight2 = message; } break;
                case CpeMessageType.BottomRight3:
                    if (!(PlayerDataHandler.Instance[p.truename].lastCPEBottomRight3 == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.truename].lastCPEBottomRight3 = message; } break;
                case CpeMessageType.Status1:
                    if (!(PlayerDataHandler.Instance[p.truename].lastCPEStatus1 == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.truename].lastCPEStatus1 = message; } break;
                case CpeMessageType.Status2:
                    if (!(PlayerDataHandler.Instance[p.truename].lastCPEStatus2 == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.truename].lastCPEStatus2 = message; } break;
                case CpeMessageType.Status3:
                    if (!(PlayerDataHandler.Instance[p.truename].lastCPEStatus3 == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.truename].lastCPEStatus3 = message; } break;
                default:
                    p.SendCpeMessage(type, message); break;
            };
        }

        public void MessageMap(CpeMessageType type, string message)
        {
            foreach (Player p in players.Values)
            {
                SendCpeMessageNoRepeat(p, type, message);
            }
        }

        #endregion
        /*************
         * COUNTDOWN *
         *************/
        #region Countdown

        private void ShowMapInfo(Player p)
        {
            string authors = string.Join(", ", map.Config.Authors.Split(',').Select(
                x => PlayerInfo.FindExact(x) == null ? x : PlayerInfo.FindExact(x).ColoredName + "%e"));

            p.Message(String.Format("Starting new round"));
            p.Message(string.Format("This map was made by {0}", authors));

            if (mapConfig.TOTAL_RATINGS == 0)
            {
                p.Message(String.Format("This map has not yet been rated"));
            } else
            {
                p.Message(String.Format("This map has a rating of {0}", mapConfig.Rating.ToString("0.00")));
            }
        }

        #endregion

        /*********
         * ROUND *
         *********/
        #region Round

        public void ShowLevel(Player p)
        {
            SendCpeMessageNoRepeat(p, CpeMessageType.Status1, String.Format("Map: {0}", map.name));
        }

        public void ShowTeamStatistics(Player p)
        {
            SendCpeMessageNoRepeat(p, CpeMessageType.Status2, String.Format("Hits team 1: Hits team 2")); // TODO: Implement
        }

        public void ShowRoundTime(Player p)
        {
            TimeSpan timeLeft = roundStart + roundTime - DateTime.Now;
            SendCpeMessageNoRepeat(p, CpeMessageType.Status3, String.Format("Round time remaining: {0}:", FormatTimeSpan(timeLeft)));
        }

        public void ShowHealth(Player p)
        {
            ushort health = PlayerDataHandler.Instance[p.truename].health;

            // Cap the health at 10
            health = health > 10 ? (ushort)10 : health;

            string blocks = "▌▌▌▌▌▌▌▌▌▌";

            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight1, String.Format("%4{0}%2{1}", blocks.Substring(health), blocks.Substring(10 - health)));
        }

        public void ShowStamina(Player p)
        {
            ushort stamina = PlayerDataHandler.Instance[p.truename].stamina;

            // Cap the stamina at 10
            stamina = stamina > 10 ? (ushort)10 : stamina;

            string blocks = "▌▌▌▌▌▌▌▌▌▌";

            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight2, String.Format("%2{1}%4{0}", blocks.Substring(stamina), blocks.Substring(10 - stamina)));
        }



        public void ShowWeaponStatus(Player p)
        {
            ushort gunStatus = PlayerDataHandler.Instance[p.truename].currentWeapon.GetStatus(WeaponHandler.Tick);

            // Cap the stamina at 10
            gunStatus = gunStatus > 10 ? (ushort)10 : gunStatus;

            string blocks = "▌▌▌▌▌▌▌▌▌▌";

            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight3, String.Format("Weapon: {2} %2{1}%4{0}",
                blocks.Substring(gunStatus),
                blocks.Substring(10 - gunStatus),
                PlayerDataHandler.Instance[p.truename].currentWeapon.name));
        }

        public void ShowWinningTeam(Player p)
        {
            p.Message("And the winners are (not implemented yet)");
        }

        #endregion

        /**********
         * VOTING *
         **********/
        #region Voting
        public void ShowVote(Player p)
        {
            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight2, "Pick a level");
            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight1, String.Format("{0}, {1}, {2}", map1, map2, map3));
        }

        public void ShowVoteTime(Player p)
        {
            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight3, String.Format("Vote time remaining: {0}", FormatTimeSpan(roundStart
                + roundTime
                + TimeSpan.FromSeconds(gameConfig.S_VOTETIME)
                - DateTime.Now)
                ));
        }

        #endregion

        #region Miscelleaneous
        public void ShowWeaponSpeed(Player p)
        {
            ushort weaponSpeed = PlayerDataHandler.Instance[p.truename].currentWeapon.WeaponSpeed;

            // Cap the stamina at 10
            weaponSpeed = weaponSpeed > 10 ? (ushort)10 : weaponSpeed;

            string blocks = "▌▌▌▌▌▌▌▌▌▌";

            SendCpeMessageNoRepeat(p, CpeMessageType.SmallAnnouncement, String.Format("%2{1}%4{0}", blocks.Substring(weaponSpeed), blocks.Substring(10 - weaponSpeed)));
        }

        public void ClearWeaponSpeed(Player p)
        {
            SendCpeMessageNoRepeat(p, CpeMessageType.SmallAnnouncement, "");
            PlayerDataHandler.Instance[p.truename].lastCPESmallAnnouncement = "";
        }
        
        public void ClearTopRight(Player p)
        {
            SendCpeMessageNoRepeat(p, CpeMessageType.Status1, "");
            SendCpeMessageNoRepeat(p, CpeMessageType.Status2, "");
            SendCpeMessageNoRepeat(p, CpeMessageType.Status3, "");
            PlayerDataHandler.Instance[p.truename].lastCPEStatus1 = "";
            PlayerDataHandler.Instance[p.truename].lastCPEStatus2 = "";
            PlayerDataHandler.Instance[p.truename].lastCPEStatus3 = "";
        }

        public void ClearBottomRight(Player p)
        {
            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight1, "");
            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight2, "");
            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight3, "");
            PlayerDataHandler.Instance[p.truename].lastCPEStatus1 = "";
            PlayerDataHandler.Instance[p.truename].lastCPEStatus2 = "";
            PlayerDataHandler.Instance[p.truename].lastCPEStatus3 = "";
        }

        public string FormatTimeSpan(TimeSpan span)
        {
            string format = @"mm\:ss";

            return span.ToString(format).TrimStart('0')
            .TrimStart(':')
            .Replace('.', ':');
        }
        #endregion
    }
}
