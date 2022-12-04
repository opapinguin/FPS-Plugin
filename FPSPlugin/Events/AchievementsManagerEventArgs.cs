using System;
using FPS.Weapons;
using MCGalaxy;

namespace FPS
{
	internal class AchievementUnlockedEventArgs : EventArgs
	{
		internal Player Player { get; set; }
		internal Achievement Achievement { get; set; }
	}
}