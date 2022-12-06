using FPS.Entities;
using MCGalaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPS.Teams;

internal static class TeamHandler
{
    private static Random r;

    internal static Team red;
    internal static Team blue;

    internal static void Activate()
    {
        red = new Team("RED");
        blue = new Team("BLUE");
        r = new Random();
    }

    internal static void Reset()
    {
        red.Reset();
        blue.Reset();
        
    }

    /// <summary>
    /// Assigns teams. First two players in this list will be assigned to red and blue respectively
    /// </summary>
    internal static void AssignTeams(List<Player> players)
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
    internal static Team GetTeam(Player p)
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
    internal static void RemovePlayer(Player p)
    {
        red.Remove(p);
        blue.Remove(p);
    }

    /// <summary>
    /// Adds a player to a team. If team is null assign randomly
    /// </summary>
    internal static void AddPlayer(Player p, string team = "")
    {
        if (InTeam(p)) {
            return;
        }

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
            FPSGame.Instance.OnPlayerJoinedTeam(p, team);
        }
        else if (team == "BLUE")
        {
            blue.Add(p);
            FPSGame.Instance.OnPlayerJoinedTeam(p, team);
        }
    }

    private static void AssignTeam(Player p, ref Team team)
    {
        team.Add(p);
    }

    private static bool InTeam(Player p)
    {

        if (red.players.Keys.Contains(p.truename)) {
            return true;
        }

        if (blue.players.Keys.Contains(p.truename))
        {
            return true;
        }
        return false;
    }
}
