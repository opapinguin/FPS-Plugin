﻿/*
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

using FPS.Entities;
using FPS.Teams;
using FPS.Weapons;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPS;

internal sealed partial class FPSMOGame
{
    internal event EventHandler<CountdownStartedEventArgs> CountdownStarted;
    private void OnCountdownStarted()
    {
        if (CountdownStarted != null)
        {
            var args = new CountdownStartedEventArgs();
            args.MapAverageRating = _databaseManager.AverageRating(map.name);
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
    private void OnCountdownTicked(int timeRemaining, bool hasEnoughPlayers)
    {
        if (CountdownTicked != null)
        {
            CountdownTickedEventArgs args = new CountdownTickedEventArgs();
            args.TimeRemaining = timeRemaining;
            args.HasEnoughPlayers = hasEnoughPlayers;

            CountdownTicked(this, args);
        }
    }

    internal event EventHandler CountdownEnded;
    private void OnCountdownEnded()
    {
        if (CountdownEnded != null)
        {
            CountdownEnded(this, EventArgs.Empty);
        }
    }

    internal event EventHandler RoundStarted;
    private void OnRoundStarted()
    {
        if (RoundStarted != null)
        {
            RoundStarted(this, EventArgs.Empty);
        }
    }

    internal event EventHandler<RoundTickedEventArgs> RoundTicked;

    private void OnRoundTicked(int timeRemaining)
    {
        if (RoundTicked != null)
        {
            var args = new RoundTickedEventArgs() { TimeRemaining = timeRemaining };
            RoundTicked(this, args);
        }
    }

    internal event EventHandler RoundEnded;
    private void OnRoundEnded()
    {
        if (RoundEnded != null)
        {
            RoundEnded(this, EventArgs.Empty);
        }
    }

    internal event EventHandler<VoteStartedEventArgs> VoteStarted;
    private void OnVoteStarted(string map1, string map2, string map3, int count)
    {
        if (VoteStarted != null)
        {
            VoteStartedEventArgs args = new VoteStartedEventArgs();
            args.Map1 = map1;
            args.Map2 = map2;
            args.Map3 = map3;
            args.Count = count;
            VoteStarted(this, args);
        }
    }

    internal event EventHandler<VoteTickedEventArgs> VoteTicked;
    private void OnVoteTicked(int timeRemaining)
    {
        if (VoteTicked != null)
        {
            VoteTicked(this, new VoteTickedEventArgs() { TimeRemaining = timeRemaining });
        }
    }

    internal event EventHandler<VoteEndedEventArgs> VoteEnded;
    private void OnVoteEnded()
    {
        if (VoteEnded != null)
        {
            VoteEndedEventArgs args = new VoteEndedEventArgs()
            {
                Map1 = map1,
                Map2 = map2,
                Map3 = map3,
                Votes1 = (int)votes1,
                Votes2 = (int)votes2,
                Votes3 = (int)votes3
            };

            VoteEnded(this, args);
        }
    }

    internal event EventHandler<PlayerJoinedEventArgs> PlayerJoined;
    private void OnPlayerJoined(Player player)
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
    private void OnGameStopped()
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

    private void HookEventHandlers()
    {
        OnPlayerMoveEvent.Register(HandlePlayerMove, Priority.High);
        OnJoinedLevelEvent.Register(HandleJoinedLevel, Priority.High);
        OnPlayerChatEvent.Register(HandePlayerChat, Priority.High);
        OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.High);
        OnPlayerChatEvent.Register(HandleVoting, Priority.High);
    }

    private void UnHookEventHandlers()
    {
        OnPlayerMoveEvent.Unregister(HandlePlayerMove);
        OnJoinedLevelEvent.Unregister(HandleJoinedLevel);
        OnPlayerChatEvent.Unregister(HandePlayerChat);
        OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
        OnPlayerChatEvent.Unregister(HandleVoting);
    }

    private void HandlePlayerDisconnect(Player p, string reason)
    {
        PlayerLeftGame(p);
    }

    private void HandePlayerChat(Player p, string message)
    {
        if (p.level != map || message.Length <= 1) return;

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
        if (prevLevel != null && prevLevel.name == map.name && level.name != map.name && !_movingToNextMap)
        {
            PlayerLeftGame(p);
        }
        else if (level.name == map.name)
        {
            PlayerJoinedGame(p);
        }
    }

    private void HandlePlayerMove(Player p, Position next, byte yaw, byte pitch, ref bool cancel)
    {
        if (p.level != map) return;

        // TODO: Maybe tidy this up?
        if (p.Game.Noclip == null) p.Game.Noclip = new MCGalaxy.Games.NoclipDetector(p);
        if (p.Game.Speed == null) p.Game.Speed = new MCGalaxy.Games.SpeedhackDetector(p);

        bool reverted = p.Game.Noclip.Detect(next) || p.Game.Speed.Detect(next, Constants.MAX_MOVE_DISTANCE);
        if (reverted) cancel = true;
    }

    private void HandleVoting(Player player, string message)
    {
        if (stage != Stage.Voting || subStage != SubStage.Middle) return;

        int mapNumber = 0;

        if (GetVotingMessage(message, ref mapNumber))
        {
            Vote(player, mapNumber);
            player.cancelchat = true;
        }
    }

    private bool GetVotingMessage(string message, ref int mapNumber)
    {
        message = message.ToLower().TrimEnd();

        if (message == "1")
        {
            mapNumber = 1;
            return true;
        }
        else if (message == "2")
        {
            mapNumber = 2;
            return true;
        }
        else if (message == "3" && map3 != null)
        {
            mapNumber = 3;
            return true;
        }

        return false;
    }

    /******************
     * HELPER METHODS *
     ******************/
    #region Helper Methods

    private void PlayerJoinedGame(Player p)
    {
        players[p.truename] = p;
        PlayerDataHandler.Instance[p.truename] = new PlayerData(p);
        TeamHandler.AddPlayer(p);
        SendBindings(p);
        OnPlayerJoined(p);
    }

    private void PlayerLeftGame(Player p)
    {
        players.Remove(p.truename);
        PlayerDataHandler.Instance.dictPlayerData.Remove(p.truename);
        TeamHandler.RemovePlayer(p);
        RemoveBindings(p);
        OnPlayerLeft(p);
    }

    #endregion
}