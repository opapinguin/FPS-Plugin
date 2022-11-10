using FPSMO.Entities;
using MCGalaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            foreach (Player p in players)
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
                    if (!(PlayerDataHandler.Instance[p.name].lastCPEAnnouncement == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.name].lastCPEAnnouncement = message; } break;
                case CpeMessageType.SmallAnnouncement:
                    if (!(PlayerDataHandler.Instance[p.name].lastCPESmallAnnouncement == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.name].lastCPESmallAnnouncement = message; } break;
                case CpeMessageType.BigAnnouncement:
                    if (!(PlayerDataHandler.Instance[p.name].lastCPEBigAnnouncement == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.name].lastCPEBigAnnouncement = message; } break;
                case CpeMessageType.BottomRight1:
                    if (!(PlayerDataHandler.Instance[p.name].lastCPEBottomRight1 == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.name].lastCPEBottomRight1 = message; } break;
                case CpeMessageType.BottomRight2:
                    if (!(PlayerDataHandler.Instance[p.name].lastCPEBottomRight2 == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.name].lastCPEBottomRight2 = message; } break;
                case CpeMessageType.BottomRight3:
                    if (!(PlayerDataHandler.Instance[p.name].lastCPEBottomRight3 == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.name].lastCPEBottomRight3 = message; } break;
                case CpeMessageType.Status1:
                    if (!(PlayerDataHandler.Instance[p.name].lastCPEStatus1 == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.name].lastCPEStatus1 = message; } break;
                case CpeMessageType.Status2:
                    if (!(PlayerDataHandler.Instance[p.name].lastCPEStatus2 == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.name].lastCPEStatus2 = message; } break;
                case CpeMessageType.Status3:
                    if (!(PlayerDataHandler.Instance[p.name].lastCPEStatus3 == message)) { p.SendCpeMessage(type, message); PlayerDataHandler.Instance[p.name].lastCPEStatus3 = message; } break;
                default:
                    p.SendCpeMessage(type, message); break;
            };
        }

        public void MessageMap(CpeMessageType type, string message)
        {
            foreach (Player p in players)
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
            p.Message(String.Format("Starting new round"));
            p.Message(string.Format("This map was made by {0}", map.Config.Authors));
            p.Message(String.Format("This map has a rating of {0}", mapConfig.rating.ToString()));
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
            ushort health = PlayerDataHandler.Instance[p.name].health;

            // Cap the health at 10
            health = health > 10 ? (ushort)10 : health;

            string blocks = "▌▌▌▌▌▌▌▌▌▌";

            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight1, String.Format("%4{0}%2{1}", blocks.Substring(10 - health), blocks.Substring(health)));
        }

        public void ShowStamina(Player p)
        {
            ushort stamina = PlayerDataHandler.Instance[p.name].stamina;

            // Cap the stamina at 10
            stamina = stamina > 10 ? (ushort)10 : stamina;

            string blocks = "▌▌▌▌▌▌▌▌▌▌";

            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight2, String.Format("%4{0}%2{1}", blocks.Substring(10 - stamina), blocks.Substring(stamina)));
        }

        public void ShowMoney(Player p)
        {
            int money = PlayerDataHandler.Instance[p.name].money;
            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight3, String.Format("Money: {0}", money));
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

        public void ClearTopRight(Player p)
        {
            SendCpeMessageNoRepeat(p, CpeMessageType.Status1, "");
            SendCpeMessageNoRepeat(p, CpeMessageType.Status2, "");
            SendCpeMessageNoRepeat(p, CpeMessageType.Status3, "");
        }

        public void ClearBottomRight(Player p)
        {
            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight1, "");
            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight2, "");
            SendCpeMessageNoRepeat(p, CpeMessageType.BottomRight3, "");
        }

        public string FormatTimeSpan(TimeSpan span)
        {
            string format = @"mm\:ss";

            return span.ToString(format).TrimStart('0')
            .TrimStart(':')
            .Replace('.', ':');
        }
    }
}
