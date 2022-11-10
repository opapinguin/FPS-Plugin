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
        public void MessageMap(CpeMessageType type, string message)
        {
            if (!bRunning) return;
            Player[] online = PlayerInfo.Online.Items;

            foreach (Player p in online)
            {
                if (p.level.MapName != map.MapName) continue;
                p.SendCpeMessage(type, message);
            }
        }

        private void ShowMapInfo(Player p)
        {
            p.Message(String.Format("Starting new round"));
            p.Message(string.Format("This map was made by {0}", map.Config.Authors));
            p.Message(String.Format("This map has a rating of {0}", mapConfig.rating.ToString()));
        }

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

        public void ShowVote(Player p)
        {
            p.SendCpeMessage(CpeMessageType.BottomRight1, "Pick a level");
            p.SendCpeMessage(CpeMessageType.BottomRight2, String.Format("{0}, {1}, {2}", map1, map2, map3));
        }

        public void ShowVoteTime(Player p)
        {
            p.SendCpeMessage(CpeMessageType.BottomRight3, String.Format("Vote time remaining: {0}", FormatTimeSpan(roundStart
                + roundTime
                + TimeSpan.FromSeconds(gameConfig.S_VOTETIME)
                - DateTime.Now)
                ));
        }
        
        public void ShowLevel(Player p)
        {
            p.SendCpeMessage(CpeMessageType.Status1, String.Format("Map: {0}", map));
        }

        public void ShowTeamStatistics(Player p)
        {
            p.SendCpeMessage(CpeMessageType.Status2, String.Format("Hits team 1: Hits team 2"));    // TODO: Finish this
        }

        public void ShowRoundTime(Player p)
        {
            TimeSpan timeLeft = roundStart + roundTime - DateTime.Now;
            p.SendCpeMessage(CpeMessageType.Status3, String.Format("Round time remaining: {0}:", FormatTimeSpan(timeLeft)));
        }

        public void ShowHealth(Player p)
        {
            p.SendCpeMessage(CpeMessageType.BottomRight1, "Health: ");
        }

        public void ShowStamina(Player p)
        {
            p.SendCpeMessage(CpeMessageType.BottomRight2, "Stamina: ");
        }

        public void ShowMoney(Player p)
        {
            p.SendCpeMessage(CpeMessageType.BottomRight3, "Money: ");
        }

        public void ClearTopRight(Player p)
        {
            p.SendCpeMessage(CpeMessageType.Status1, "");
            p.SendCpeMessage(CpeMessageType.Status2, "");
            p.SendCpeMessage(CpeMessageType.Status3, "");
        }

        public void ClearBottomRight(Player p)
        {
            p.SendCpeMessage(CpeMessageType.BottomRight1, "");
            p.SendCpeMessage(CpeMessageType.BottomRight2, "");
            p.SendCpeMessage(CpeMessageType.BottomRight3, "");
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
