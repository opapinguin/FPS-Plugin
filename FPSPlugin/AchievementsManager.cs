using System;
using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.SQL;

namespace FPS;

internal class AchievementsManager
{
    private readonly List<Achievement> _achievements = new List<Achievement>()
    {
        new Achievement()
        {
            Reward = 15,
            Name = "Achievement tester",
            Description = "Run the command /achievementtest"
        },
    };

    internal List<Achievement> Achievements => _achievements;

    internal AchievementsManager()
    {
        // TODO
    }

    private Achievement ResolveAchievement(string achievementName)
    {
        foreach (Achievement achievement in _achievements)
        {
            if (achievement.Name == achievementName)
                return achievement;
        }

        throw new ArgumentException($"Could not resolve achievement {achievementName}");
    }

    internal void TriggerTestAchievement(Player player)
    {
        Achievement testAchievement = ResolveAchievement("Achievement tester");
        OnAchievementUnlocked(player, testAchievement);
    }

    internal event EventHandler<AchievementUnlockedEventArgs> AchievementUnlocked;
    private void OnAchievementUnlocked(Player player, Achievement achievement)
    {
        if (AlreadyHas(player, achievement)) return;

        if (AchievementUnlocked != null)
        {
            var args = new AchievementUnlockedEventArgs()
            {
                Player = player,
                Achievement = achievement
            };

            AchievementUnlocked(this, args);
        }
    }

    private bool AlreadyHas(Player player, Achievement achievement)
    {
        List<string[]> matchingRows = 
            Database.GetRows("PlayersAchievements", "AchievementName", "WHERE Player=@0", player.truename);

        foreach (string[] row in matchingRows)
        {
            if (row[0] == achievement.Name) return true;
        }

        return false;
    }

    internal void Observe(FPSMOGame game)
    {
        // Subscribe to events here
    }

    internal void Unobserve(FPSMOGame game)
    {
        // Unsubscribe from events here
    }
}
