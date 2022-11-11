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
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPSMO
{
    public sealed partial class FPSMOGame
    {
        private void HookEventHandlers()
        {
            OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.High);
            OnPlayerMoveEvent.Register(HandlePlayerMove, Priority.High);
            OnJoinedLevelEvent.Register(HandleJoinedLevel, Priority.High);
            OnPlayerChatEvent.Register(HandePlayerChat, Priority.High);
            OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.High);
        }

        private void UnHookEventHandlers()
        {
            OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
            OnPlayerMoveEvent.Unregister(HandlePlayerMove);
            OnJoinedLevelEvent.Unregister(HandleJoinedLevel);
            OnPlayerChatEvent.Unregister(HandePlayerChat);
            OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
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
            if (prevLevel == map && level != map)
            {
                PlayerLeftGame(p);
                return;
            }

            if (level == map)
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

        /******************
         * HELPER METHODS *
         ******************/
        #region Helper Methods

        private void PlayerJoinedGame(Player p)
        {
            players.Add(p);
            PlayerDataHandler.Instance[p.name] = new PlayerData(p);
            SendBindings(p);
        }

        private void PlayerLeftGame(Player p)
        {
            players.Remove(p);
            // TODO: Remove bindings here
            PlayerDataHandler.Instance.dictPlayerData.Remove(p.name);
            RemoveBindings(p);
        }

        #endregion
    }
}
