using System;
using MCGalaxy;

namespace FPSMO
{
	internal class CountdownTickedEventArgs : EventArgs
	{
		internal int TimeRemaining { get; set; }
		internal bool HasEnoughPlayers { get; set; }
	}

    internal class RoundTickedEventArgs : EventArgs
    {
		internal int TimeRemaining { get; set; }
    }

    internal class VoteStartedEventArgs : EventArgs
	{
		internal string Map1 { get; set; }
        internal string Map2 { get; set; }
        internal string Map3 { get; set; }
    }

	internal class VoteTickedEventArgs : EventArgs
	{
		internal int TimeRemaining { get; set; }
    }

	internal class VoteEndedEventArgs : EventArgs
	{
		internal string Map1 { get; set; }
        internal string Map2 { get; set; }
        internal string Map3 { get; set; }
        internal int Votes1 { get; set; }
        internal int Votes2 { get; set; }
        internal int Votes3 { get; set; }
    }

	internal class PlayerJoinedEventArgs : EventArgs
	{
		internal Player Player { get; set; }
    }

	internal class WeaponSpeedChangedEventArgs : EventArgs
	{
		internal Player Player { get; set; }
        internal int Amount { get; set; }
    }

	internal class PlayerJoinedTeamEventArgs : EventArgs
	{
		internal Player Player { get; set; }
		internal string TeamName { get; set; }
	}

	internal class PlayerKilledEventArgs : EventArgs
	{
		internal Player Killer { get; set; }
		internal Player Victim { get; set; }
	}
}