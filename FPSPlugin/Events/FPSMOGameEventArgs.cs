using System;
using FPS.Weapons;
using MCGalaxy;

namespace FPS;

internal class WeaponChangedEventArgs : EventArgs
{
    internal Player p { get; set; }
}

internal class WeaponStatusChangedEventArgs : EventArgs
{
    internal int Status { get; set; }
    internal Player Player { get; set; }
}

internal class PlayerShotWeaponEventArgs : EventArgs
{
    internal Player Player { get; set; }
}

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
    internal int Count { get; set; }
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

internal class PlayerLeftEventArgs : EventArgs
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

internal class PlayerHitEventArgs : EventArgs
{
    internal Player Shooter { get; set; }
    internal Player Victim { get; set; }
    internal WeaponEntity WeaponEntity { get; set; }
}

internal class CountdownStartedEventArgs : EventArgs
{
    internal float? MapAverageRating { get; set; }
}
