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

using FPSMO.Entities;
using FPSMO.Teams;
using FPSMO.Weapons;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPSMO
{
    internal sealed partial class FPSMOGame
    {
        private void HookEventHandlers()
        {
            OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.High);
            OnPlayerMoveEvent.Register(HandlePlayerMove, Priority.High);
            OnJoinedLevelEvent.Register(HandleJoinedLevel, Priority.High);
            OnPlayerChatEvent.Register(HandePlayerChat, Priority.High);
            OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.High);
            OnPlayerChatEvent.Register(HandleVoting, Priority.High);
        }

        private void UnHookEventHandlers()
        {
            OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
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
            if (prevLevel != null && prevLevel.name == map.name && level.name != map.name)   // Maps are not interfaced as comparable
            {
                PlayerLeftGame(p);
                return;
            }

            if (level.name == map.name)
            {
                PlayerJoinedGame(p);
                switch (stage)
                {
                    case Stage.Countdown:
                        ShowMapInfo(p);
                        break;
                    case Stage.Voting:
                        ShowVote(p);
                        break;
                }
            }
        }

        private void HandlePlayerMove(Player p, Position next, byte yaw, byte pitch, ref bool cancel)
        {
            if (p.level != map) return;

            // TODO: Maybe tidy this up?
            if (p.Game.Noclip == null) p.Game.Noclip = new MCGalaxy.Games.NoclipDetector(p);
            if (p.Game.Speed == null) p.Game.Speed = new MCGalaxy.Games.SpeedhackDetector(p);

            bool reverted = p.Game.Noclip.Detect(next) || p.Game.Speed.Detect(next, gameConfig.MAX_MOVE_DISTANCE);
            if (reverted) cancel = true;
        }

        private void HandlePlayerConnect(Player p)
        {
            PlayerJoinedGame(p);
            // TODO: Add round recorder here
        }

        private void HandleVoting(Player p, string msg)
        {
            if (FPSMOGame.Instance.stage != Stage.Voting || FPSMOGame.Instance.subStage != SubStage.Middle) { return; }
            msg = msg.ToLower().TrimEnd();
            UpdateVote(msg, p, "1", "one", ref votes1);
            UpdateVote(msg, p, "2", "two", ref votes2);
            UpdateVote(msg, p, "3", "three", ref votes3);
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
        }

        private void PlayerLeftGame(Player p)
        {
            players.Remove(p.truename);
            PlayerDataHandler.Instance.dictPlayerData.Remove(p.truename);
            TeamHandler.RemovePlayer(p);
            RemoveBindings(p);
        }

        public void HandleHit(WeaponEntity we, Player shooter, Player victim)
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
            if (we.damage >= pd_victim.health) {
                HandleDeath(shooter, victim); return;
            } else
            {
                pd_victim.health -= (ushort)we.damage;
            }
            


            PlayerDataHandler.Instance[shooter.truename] = pd_shooter;
            PlayerDataHandler.Instance[victim.truename] = pd_victim;
        }

        public void HandleDeath(Player shooter, Player victim)
        {
            PlayerData pd_shooter = PlayerDataHandler.Instance[shooter.truename];
            PlayerData pd_victim = PlayerDataHandler.Instance[victim.truename];

            pd_shooter.kills += 1;
            pd_victim.deaths += 1;

            PlayerDataHandler.Instance[shooter.truename] = pd_shooter;
            PlayerDataHandler.Instance[victim.truename] = pd_victim;

            FPSMOGame.Instance.MessageMap(CpeMessageType.Normal, String.Format("{0} killed {1}", shooter.ColoredName, victim.ColoredName));

            PlayerActions.Respawn(victim);
        }

        #endregion
    }
}
