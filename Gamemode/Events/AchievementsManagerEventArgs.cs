using System;
using FPSMO.Weapons;
using MCGalaxy;

namespace FPSMO
{
	internal class AchievementUnlockedEventArgs : EventArgs
	{
		internal Player Player { get; set; }
		internal Achievement Achievement { get; set; }
	}
}