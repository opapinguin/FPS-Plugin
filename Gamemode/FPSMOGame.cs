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
using FPSMO.Teams;
using FPSMO.DB;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace FPSMO
{
    /// <summary>
    /// Contains the basic gameloop
    /// 
    /// A game is divided into three stages: countdown, round, and voting. I've tried my best to decouple these as much as possible
    /// We organize each of these stages into three sub-stages: beginning, middle and end
    /// The logic for each stage can be found in FPSMOGame.Countdown, FPSMOGame.Round and FPSMOGame.Voting
    /// </summary>
    internal sealed partial class FPSMOGame
    {
        /*************************
         * SINGLETON BOILERPLATE *
         *************************/
        #region Singleton Boilerplate
        private static FPSMOGame instance = null;
        private static readonly object padlock = new object();

        private FPSMOGame() {}

        internal static FPSMOGame Instance
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
        internal bool bRunning;   // Default = false

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

        internal MapData mapData;
        internal LevelPicker LevelPicker { get; set; }
        private GameProperties _gameProperties;
        private DatabaseManager _databaseManager;
        private bool _movingToNextMap = false;

        internal void SetDatabaseManager(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        internal void SetGameProperties(GameProperties gameProperties)
        {
            _gameProperties = gameProperties;
        }

        internal Dictionary<string, Player> players = new Dictionary<string, Player>();
        internal Level map;
        DateTime roundStart;
        TimeSpan roundTime;
        internal DateTime RoundEnd { get { return roundStart + roundTime; } }
        internal DateTime RoundStart { get { return roundStart; } }

        #endregion
        /******************
         * MAIN GAME LOOP *
         ******************/
        #region Game Loop

        internal void Start()
        {
            string[] mapPool = _databaseManager.GetMapsPool();

            if (mapPool.Length == 0)
            {
                Logger.Log(LogType.Warning, "Cannot start the game: the map pool is empty.");
                return;
            }

            Random random = new Random();
            int mapIndex = random.Next(mapPool.Length);

            Start(mapPool[mapIndex]);
        }

        internal void Start(string mapName)
        {
            string[] mapPool = _databaseManager.GetMapsPool();

            if (!mapPool.CaselessContains(mapName))
            {
                Logger.Log(LogType.Warning, "Could not start game on {mapName}: it is not in the pool.");
                return;
            }

            players = new Dictionary<string, Player>();
            HookEventHandlers();
            StartRound(mapName);

            bRunning = true;

            Thread t = new Thread(Run) { Name = "FPSMO" };
            t.Start();  // Automatically aborts when Run() returns
            Chat.MessageAll($"&SFPS was started on &T{mapName}&S.");
        }

        private void StartRound(string mapName)
        {
            map = Level.Load(mapName);
            mapData = _databaseManager.GetMapData(map.name) ?? MapData.Default(mapName);
            LevelPicker.Register(mapName);

            TeamHandler.Activate();
            PlayerDataHandler.Instance.ResetPlayerData();
            ResetVotes();

            if (mapData.RoundDurationSeconds is null)
                roundTime = TimeSpan.FromSeconds(_gameProperties.DefaultRoundDurationSeconds);
            else
                roundTime = TimeSpan.FromSeconds((double)mapData.RoundDurationSeconds);

            MovePlayersToNextMap(mapName);
            CountStandingPlayersAsJoining(mapName);

            stage = Stage.Countdown;
            subStage = SubStage.Begin;
        }

        private void CountStandingPlayersAsJoining(string levelName)
        {
            foreach (Player player in PlayerInfo.Online.Items)
            {
                if (player.level.name == levelName)
                    PlayerJoinedGame(player);
            }
        }
        
        internal void Stop()
        {
            Dictionary<string, Player> playersCopy = new Dictionary<string, Player>(players);

            foreach (Player p in playersCopy.Values)
            {
                PlayerLeftGame(p);
            }

            UnHookEventHandlers();
            bRunning = false;

            // TODO: Remove animations
            WeaponAnimsHandler.Deactivate();
            WeaponHandler.Deactivate();

            PlayerDataHandler.Instance.Deactivate();
            Chat.MessageAll($"&SFPS was stopped.");
        }

        internal void Run()
        {
            while (bRunning)
            {
                switch (stage)
                {
                    case Stage.Countdown:
                        if (mapData.CountdownTimeSeconds is null)
                        {
                            UpdateCountdown(_gameProperties.CountdownDurationSeconds, subStage);
                        }
                        else
                        {
                            UpdateCountdown((uint)mapData.CountdownTimeSeconds, subStage);
                        }
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
                    MiddleCountdown(delay);
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
