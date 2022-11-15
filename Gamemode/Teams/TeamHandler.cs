using FPSMO.Entities;
using MCGalaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPSMO.Teams
{
    internal static class TeamHandler
    {
        private static Random r;

        public static Team red;
        public static Team blue;

        public static void Activate()
        {
            red = new Team("RED");
            blue = new Team("BLUE");
            r = new Random();
        }

        public static void Reset()
        {
            red.Reset();
            blue.Reset();
            
        }

        /// <summary>
        /// Assigns teams. First two players in this list will be assigned to red and blue respectively
        /// </summary>
        public static void AssignTeams(List<Player> players)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (i == 0) { AssignTeam(players[i], ref red); continue; }
                if (i == 1) { AssignTeam(players[i], ref blue); continue; }

                // Randomly assign a team
                int index = r.Next(0, 2);

                if (index == 0)
                {
                    AssignTeam(players[i], ref red);
                }
                else
                {
                    AssignTeam(players[i], ref blue);
                }
            }
        }

        /// <summary>
        /// Gets the team. Returns null if player not playing or not in a team
        /// </summary>
        public static Team GetTeam(Player p)
        {
            PlayerData pData = PlayerDataHandler.Instance[p.truename];
            if (pData == null) { return null; }

            if (pData.team == "NONE")
            {
                return null;
            } else if (pData.team == "RED")
            {
                return red;
            } else if (pData.team == "BLUE")
            {
                return blue;
            }
            return null;
        }

        /// <summary>
        /// Removes player from all teams
        /// </summary>
        public static void RemovePlayer(Player p)
        {
            red.Remove(p);
            blue.Remove(p);
        }

        /// <summary>
        /// Adds a player to a team. If team is null assign randomly
        /// </summary>
        public static void AddPlayer(Player p, string team = "")
        {
            // As a rule, add to blue if there's no blue members and red if there's no red members
            if (red.Count == 0)
            {
                team = "RED";
            } else if (blue.Count == 0)
            {
                team = "BLUE";
            }

            // Set team randomly if team == ""
            if (team == "")
            {
                int index = r.Next(0, 2);
                if (index == 0)
                {
                    team = "RED";
                } else
                {
                    team = "BLUE";
                }
            }

            // Set the team
            if (team == "RED")
            {
                red.Add(p);
                FPSMOGame.Instance.OnPlayerJoinedTeam(p, team);
            }
            else if (team == "BLUE")
            {
                blue.Add(p);
                FPSMOGame.Instance.OnPlayerJoinedTeam(p, team);
            }
        }

        private static void AssignTeam(Player p, ref Team team)
        {
            team.Add(p);
        }
    }
}
