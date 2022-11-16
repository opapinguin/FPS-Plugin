using System;
using MCGalaxy;

namespace FPSMO.Commands
{
	internal class CmdAchievementTest : Command2
	{
        public override string name { get { return "achievementtest"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override LevelPermission defaultRank { get { return LevelPermission.AdvBuilder; } }

        private readonly AchievementsManager _achievementsManager;

        internal CmdAchievementTest(AchievementsManager manager)
        {
            _achievementsManager = manager;
        }

        public override void Use(Player player, string message)
        {
            _achievementsManager.TriggerTestAchievement(player);
        }

        public override void Help(Player player)
        {
            player.Message("&T/AchievementTest &H- Command used for testing achievements.");
        }
    }
}

