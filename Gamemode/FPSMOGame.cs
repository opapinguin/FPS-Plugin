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

using MCGalaxy.Tasks;
using MCGalaxy.Events;
using MCGalaxy.Events.PlayerEvents;
using System;
using System.Collections.Generic;
using System.Threading;
using MCGalaxy;
using FPSMO.Configuration;
using FPSMO.Weapons;
using FPSMO.Entities;

namespace FPSMO
{
    /// <summary>
    /// Contains the basic gameloop
    /// 
    /// A game is divided into three stages: countdown, round, and voting. I've tried my best to decouple these as much as possible
    /// We organize each of these stages into three sub-stages: beginning, middle and end
    /// The logic for each stage can be found in FPSMOGame.Countdown, FPSMOGame.Round and FPSMOGame.Voting
    /// </summary>
    public sealed partial class FPSMOGame
    {
        /*************************
         * SINGLETON BOILERPLATE *
         *************************/
        #region Singleton Boilerplate
        private static FPSMOGame instance = null;
        private static readonly object padlock = new object();

        FPSMOGame()
        {
        }

        public static FPSMOGame Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new FPSMOGame();
                    }
                    return instance;
                }
            }
        }

        #endregion
        /***************
         * GAME FIELDS *
         ***************/
        #region Game Fields
        private const int MS_ROUND_TICK = 200;  // Quite large but really nothing special needs handling during the round in the main loop
        public bool bRunning;   // Default = false
        
        internal enum Stage
        {
            Countdown,
            Round,
            Voting
        }
        internal enum SubStage
        {
            Begin,
            Middle,
            End
        }

        internal Stage stage;
        internal SubStage subStage;

        internal FPSMOMapConfig mapConfig;
        internal FPSMOGameConfig gameConfig;

        public Dictionary<string, Player> players = new Dictionary<string, Player>();
        public Level map;
        DateTime roundStart;
        TimeSpan roundTime;

        #endregion
        /******************
         * MAIN GAME LOOP *
         ******************/
        #region Game Loop

        public void Start(string mapName = "")
        {
            // Hook eventhandlers
            HookEventHandlers();
            ActivateTasks();

            // Prepare the game configuration
            FPSMOConfig<FPSMOMapConfig>.dir = "FPSMOConfig/Maps";
            FPSMOConfig<FPSMOPlayerConfig>.dir = "FPSMOConfig/Player";
            FPSMOConfig<FPSMOGameConfig>.dir = "FPSMOConfig/Game";

            // Create a game configuration if it doesn't already exist
            FPSMOConfig<FPSMOGameConfig>.Create("Config", new FPSMOGameConfig(true));
            gameConfig = FPSMOConfig<FPSMOGameConfig>.Read("Config");

            // Pick a level
            LevelPicker.Activate();
            if (mapName == "")
            {
                map = Level.Load(LevelPicker.PopAndPush());
            } else
            {
                map = Level.Load(mapName);
            }

            // Create a map configuration if it doesn't already exist. Defaults to main level if no levels have been added
            FPSMOConfig<FPSMOMapConfig>.Create(Server.Config.MainLevel, new FPSMOMapConfig(gameConfig.DEFAULT_ROUNDTIME_S));
            mapConfig = FPSMOConfig<FPSMOMapConfig>.Read(map.name);

            roundTime = TimeSpan.FromSeconds(mapConfig.RoundTime_S);

            // Add the players to the game
            players = new Dictionary<string, Player>();
            foreach (Player p in PlayerInfo.Online.Items)
            {
                if (p.level.name == map.name)
                {
                    PlayerJoinedGame(p);
                }
            }

            // Start the game
            stage = Stage.Countdown;
            subStage = SubStage.Begin;

            bRunning = true;

            Thread t = new Thread(Run)
            {
                Name = "FPSMO"
            };
            t.Start();  // Automatically aborts when Run() returns
        }
        
        public void Stop()
        {
            Dictionary<string, Player> playersCopy = new Dictionary<string, Player>(players);

            ShowToAll(ClearBottomRight);
            ShowToAll(ClearTopRight);

            foreach (Player p in playersCopy.Values)
            {
                PlayerLeftGame(p);
            }

            UnHookEventHandlers();
            DeactivateTasks();
            bRunning = false;

            // TODO: Remove animations

            Chat.MessageAll("Parkour Game Stopped");

            WeaponAnimsHandler.Deactivate();
            PlayerDataHandler.Instance.Deactivate();
        }

        public void Run()
        {
            while (bRunning)
            {
                switch (stage)
                {
                    case Stage.Countdown:
                        UpdateCountdown(gameConfig.S_COUNTDOWNTIME, subStage);
                        break;
                    case Stage.Round:
                        UpdateRound(subStage);
                        break;
                    case Stage.Voting:
                        UpdateVoting(subStage);
                        break;
                }
            }
        }

        #endregion
        /*******************
         * COUNTDOWN LOGIC *
         *******************/
        #region Countdown

        private void UpdateCountdown(uint delay, SubStage subStage)
        {
            switch (subStage)
            {
                case SubStage.Begin:
                    BeginCountdown(delay);
                    break;
                case SubStage.Middle:
                    MiddleCountdown("&4Starting in &f{0} &4seconds", delay, 10);
                    break;
                case SubStage.End:
                    EndCountdown();
                    break;
            }
        }
        
        #endregion
        /***************
         * ROUND LOGIC *
         ***************/
        #region Round
        private void UpdateRound(SubStage subStage)
        {
            switch (subStage)
            {
                case SubStage.Begin:
                    BeginRound();
                    break;
                case SubStage.Middle:
                    MiddleRound();
                    break;
                case SubStage.End:
                    EndRound();
                    break;
            }
        }

        #endregion
        /****************
         * VOTING LOGIC *
         ****************/
        #region Voting
        private void UpdateVoting(SubStage subStage)
        {
            switch (subStage)
            {
                case SubStage.Begin:
                    BeginVoting();
                    break;
                case SubStage.Middle:
                    MiddleVoting();
                    break;
                case SubStage.End:
                    EndVoting();
                    break;
            }
        }

        #endregion
    }

}
