using System;
using System.Collections.Generic;
using MCGalaxy;

namespace FPS.Commands;

internal class CmdAchievements : Command2
{
    public override string name { get { return "achievements"; } }
    public override string shortcut { get { return "awards"; } }
    public override string type { get { return CommandTypes.Games; } }

    private readonly AchievementsManager _achievementsManager;

    internal CmdAchievements(AchievementsManager manager)
    {
        _achievementsManager = manager;
    }

    public override void Use(Player p, string message)
    {
        List<Achievement> achievements = _achievementsManager.Achievements;

        foreach (Achievement achievement in achievements)
        {
            p.Message(AchievementToString(achievement));
        }
    }

    public override void Help(Player p)
    {
        p.Message("&T/Achievements &H- Lists achievements");
    }

    private string AchievementToString(Achievement achievement)
    {
        return $"&H{achievement.Name} - {achievement.Description}";
    }
}
