/*
Copyright 2022 WOCC Team

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge,
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using FPS.Entities;
using FPS.Teams;
using FPS.Weapons;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;

namespace FPS;

internal sealed partial class FPSGame
{
    internal event EventHandler<CountdownStartedEventArgs> CountdownStarted;
    internal void OnCountdownStarted()
    {
        if (CountdownStarted != null)
        {
            var args = new CountdownStartedEventArgs();
            args.MapAverageRating = _databaseManager.AverageRating(Map.name);
            CountdownStarted(this, args);
        }
    }

    internal event EventHandler<WeaponChangedEventArgs> WeaponChanged;
    internal void OnWeaponChanged(Player p)
    {
        if (WeaponChanged != null)
        {
            WeaponChangedEventArgs args = new WeaponChangedEventArgs();
            args.p = p;

            WeaponChanged(this, args);
        }
    }

    internal event EventHandler<WeaponStatusChangedEventArgs> WeaponStatusChanged;
    internal void OnWeaponStatusChanged(Player p)
    {
        if (WeaponStatusChanged != null)
        {
            WeaponStatusChangedEventArgs args = new WeaponStatusChangedEventArgs();
            args.Status = PlayerDataHandler.Instance[p.truename].currentWeapon.GetStatus(WeaponHandler.Tick);
            args.Player = p;

            WeaponStatusChanged(this, args);
        }
    }

    internal event EventHandler<PlayerShotWeaponEventArgs> PlayerShotWeapon;
    internal void OnPlayerShotWeapon(Player p)
    {
        if (PlayerShotWeapon != null)
        {
            PlayerShotWeaponEventArgs args = new PlayerShotWeaponEventArgs();
            args.Player = p;

            PlayerShotWeapon(this, args);
        }
    }

    internal event EventHandler<CountdownTickedEventArgs> CountdownTicked;
    internal void OnCountdownTicked(int timeRemaining)
    {
        if (CountdownTicked != null)
        {
            CountdownTickedEventArgs args = new CountdownTickedEventArgs();
            args.TimeRemaining = timeRemaining;

            CountdownTicked(this, args);
        }
    }

    internal event EventHandler CountdownEnded;
    internal void OnCountdownEnded()
    {
        if (CountdownEnded != null)
        {
            CountdownEnded(this, EventArgs.Empty);
        }
    }

    internal event EventHandler RoundStarted;
    internal void OnRoundStarted()
    {
        if (RoundStarted != null)
        {
            RoundStarted(this, EventArgs.Empty);
        }
    }

    internal event EventHandler<RoundTickedEventArgs> RoundTicked;
    internal void OnRoundTicked(int timeRemaining)
    {
        if (RoundTicked != null)
        {
            var args = new RoundTickedEventArgs() { TimeRemaining = timeRemaining };
            RoundTicked(this, args);
        }
    }

    internal event EventHandler RoundEnded;
    internal void OnRoundEnded()
    {
        if (RoundEnded != null)
        {
            RoundEnded(this, EventArgs.Empty);
        }
    }

    internal event EventHandler<VoteStartedEventArgs> VoteStarted;
    internal void OnVoteStarted(List<string> maps)
    {
        if (VoteStarted != null)
        {
            var args = new VoteStartedEventArgs();
            args.Map1 = maps[0];
            args.Map2 = maps[1];

            if (maps.Count == 3)
                args.Map3 = maps[2];

            VoteStarted(this, args);
        }
    }

    internal event EventHandler<VoteTickedEventArgs> VoteTicked;
    internal void OnVoteTicked(int timeRemaining)
    {
        if (VoteTicked != null)
        {
            VoteTicked(this, new VoteTickedEventArgs() { TimeRemaining = timeRemaining });
        }
    }

    internal event EventHandler<VoteEndedEventArgs> VoteEnded;
    internal void OnVoteEnded(string[] maps, int[] votes)
    {
        if (VoteEnded != null)
        {
            VoteEndedEventArgs args = new VoteEndedEventArgs()
            {
                Maps = maps,
                Votes = votes
            };

            VoteEnded(this, args);
        }
    }

    internal event EventHandler<PlayerJoinedEventArgs> PlayerJoined;
    internal void OnPlayerJoined(Player player)
    {
        if (PlayerJoined != null)
        {
            PlayerJoinedEventArgs args = new PlayerJoinedEventArgs()
            {
                Player = player
            };

            PlayerJoined(this, args);
        }
    }

    internal event EventHandler<PlayerLeftEventArgs> PlayerLeft;
    internal void OnPlayerLeft(Player player)
    {
        if (PlayerLeft != null)
        {
            var args = new PlayerLeftEventArgs() { Player = player };
            PlayerLeft(this, args);
        }
    }

    internal event EventHandler GameStopped;
    internal void OnGameStopped()
    {
        if (GameStopped != null)
        {
            GameStopped(this, EventArgs.Empty);
        }
    }

    internal event EventHandler<WeaponSpeedChangedEventArgs> WeaponSpeedChanged;
    internal void OnWeaponSpeedChanged(Player player, int amount)
    {
        if (WeaponSpeedChanged != null)
        {
            var args = new WeaponSpeedChangedEventArgs()
            {
                Player = player,
                Amount = amount
            };

            WeaponSpeedChanged(this, args);
        }
    }

    internal event EventHandler<PlayerJoinedTeamEventArgs> PlayerJoinedTeam;
    internal void OnPlayerJoinedTeam(Player player, string teamName)
    {
        if (PlayerJoinedTeam != null)
        {
            var args = new PlayerJoinedTeamEventArgs()
            {
                Player = player,
                TeamName = teamName
            };

            PlayerJoinedTeam(this, args);
        }
    }

    internal event EventHandler<PlayerHitEventArgs> PlayerHit;
    internal void OnPlayerHitPlayer(Player shooter, Player victim, WeaponEntity we)
    {
        // Find the player data
        PlayerData pd_shooter = PlayerDataHandler.Instance[shooter.truename];
        PlayerData pd_victim = PlayerDataHandler.Instance[victim.truename];

        // Just quick check if not null i.e. playerdata actually exists
        if (pd_shooter == null || pd_victim == null) { return; }

        // Change playerdata
        pd_shooter.hitsGiven += 1;
        pd_victim.hitsReceived += 1;

        // Handle death or just plain hit
        if (we.damage >= pd_victim.health)
        {
            OnPlayerKilled(shooter, victim); return;
        }
        else
        {
            pd_victim.health -= (ushort)we.damage;
        }

        PlayerDataHandler.Instance[shooter.truename] = pd_shooter;
        PlayerDataHandler.Instance[victim.truename] = pd_victim;

        if (PlayerHit != null)
        {
            var args = new PlayerHitEventArgs()
            {
                Shooter = shooter,
                Victim = victim
            };
            PlayerHit(this, args);
        }
    }

    internal event EventHandler<PlayerKilledEventArgs> PlayerKilled;
    internal void OnPlayerKilled(Player killer, Player victim)
    {
        PlayerData pd_shooter = PlayerDataHandler.Instance[killer.truename];
        PlayerData pd_victim = PlayerDataHandler.Instance[victim.truename];

        pd_shooter.kills += 1;
        pd_victim.deaths += 1;

        PlayerDataHandler.Instance[killer.truename] = pd_shooter;
        PlayerDataHandler.Instance[victim.truename] = pd_victim;

        if (PlayerKilled != null)
        {
            var args = new PlayerKilledEventArgs()
            {
                Killer = killer,
                Victim = victim
            };

            PlayerKilled(this, args);
        }

        PlayerActions.Respawn(victim);
    }

    internal event EventHandler CountdownFailed;
    internal void OnCountdownFailed()
    {
        if (CountdownFailed != null)
        {
            CountdownFailed(this, EventArgs.Empty);
        }
    }

    private void HookEventHandlers()
    {
        OnPlayerMoveEvent.Register(HandlePlayerMove, Priority.High);
        OnJoinedLevelEvent.Register(HandleJoinedLevel, Priority.High);
        OnPlayerChatEvent.Register(HandlePlayerChat, Priority.High);
        OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.High);
    }

    private void UnHookEventHandlers()
    {
        OnPlayerMoveEvent.Unregister(HandlePlayerMove);
        OnJoinedLevelEvent.Unregister(HandleJoinedLevel);
        OnPlayerChatEvent.Unregister(HandlePlayerChat);
        OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
    }

    private void HandlePlayerDisconnect(Player p, string reason)
    {
        PlayerLeftGame(p);
    }

    private void HandlePlayerChat(Player p, string message)
    {
        if (p.level != Map || message.Length <= 1) return;

        if (message[0] == '-')
        {
            if (p.Game.Team == null)
            {
                p.Message("You are not on a team, so cannot send a team message.");
            }
            else
            {
                p.Game.Team.Message(p, message.Substring(1));
            }
            p.cancelchat = true;
        }
    }

    private void HandleJoinedLevel(Player p, Level prevLevel, Level level, ref bool announce)
    {
        if (prevLevel != null && prevLevel.name == Map.name && level.name != Map.name)
        {
            PlayerLeftGame(p);
        }
        else if (level.name == Map.name)
        {
            PlayerJoinedGame(p);
        }
    }

    private void HandlePlayerMove(Player p, Position next, byte yaw, byte pitch, ref bool cancel)
    {
        if (p.level != Map) return;

        // TODO: Maybe tidy this up?
        if (p.Game.Noclip == null) p.Game.Noclip = new MCGalaxy.Games.NoclipDetector(p);
        if (p.Game.Speed == null) p.Game.Speed = new MCGalaxy.Games.SpeedhackDetector(p);

        bool reverted = p.Game.Noclip.Detect(next) || p.Game.Speed.Detect(next, Constants.MAX_MOVE_DISTANCE);
        if (reverted) cancel = true;
    }

    /******************
     * HELPER METHODS *
     ******************/
    #region Helper Methods

    internal void PlayerJoinedGame(Player p)
    {
        Players[p.truename] = p;
        PlayerDataHandler.Instance[p.truename] = new PlayerData(p);
        TeamHandler.AddPlayer(p);
        SendBindings(p);
        OnPlayerJoined(p);
    }

    internal void PlayerLeftGame(Player p)
    {
        Players.Remove(p.truename);
        PlayerDataHandler.Instance.dictPlayerData.Remove(p.truename);
        TeamHandler.RemovePlayer(p);
        RemoveBindings(p);
        OnPlayerLeft(p);
    }

    #endregion
}
