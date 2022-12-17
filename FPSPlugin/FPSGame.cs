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
using System.Threading;
using FPS.Configuration;
using FPS.DB;
using FPS.Entities;
using FPS.Weapons;
using MCGalaxy;
using MCGalaxy.Network;

namespace FPS;

internal sealed partial class FPSGame
{
    private static FPSGame instance = null;
    private static readonly object padlock = new object();

    private FPSGame() {}

    internal static FPSGame Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new FPSGame();
                }
                return instance;
            }
        }
    }

    internal bool IsRunning { get; set; }
    internal bool IsShootingEnabled { get; set; }
    internal bool IsTeamSwappingAllowed { get; set; }
    internal MapData MapData { get; set; }
    internal LevelPicker LevelPicker { get; set; }
    internal int VoteDurationSeconds { get; set; }
    internal Dictionary<string, Player> Players = new Dictionary<string, Player>();
    internal Level Map;

    private GameProperties _gameProperties;
    private DatabaseManager _databaseManager;
    private GameState _gameState;
    private string _defaultServerName = Server.Config.Name;
    private Timer _mainLoopTimer;
    private object _mainLoopLocker = new();

    private const int GameTickMilliseconds = 50;

    internal void SetDatabaseManager(DatabaseManager databaseManager)
    {
        _databaseManager = databaseManager;
    }

    internal void SetGameProperties(GameProperties gameProperties)
    {
        _gameProperties = gameProperties;
        VoteDurationSeconds = (int)_gameProperties.VoteDurationSeconds;
    }

    internal void SetState(GameState gameState)
    {
        if (_gameState != null) _gameState.Exit();

        _gameState = gameState;
        _gameState.Enter();
    }

    internal void Start()
    {
        string[] mapsPool = _databaseManager.GetMapsPool();

        if (mapsPool.Length == 0)
        {
            Logger.Log(LogType.Warning, "Cannot start the game: the maps pool is empty.");
            return;
        }

        Random random = new();
        int mapIndex = random.Next(mapsPool.Length);

        Start(mapsPool[mapIndex]);
    }

    internal void Start(string mapName)
    {
        string[] mapsPool = _databaseManager.GetMapsPool();

        if (!mapsPool.CaselessContains(mapName))
        {
            Logger.Log(LogType.Warning, "Could not start game on {mapName}: it is not in the pool.");
            return;
        }

        Players = new Dictionary<string, Player>();
        HookEventHandlers();

        IsRunning = true;
        SetState(new StateLoading(this, mapName));

        _mainLoopTimer = new Timer(MainLoop,
            state: null, dueTime: GameTickMilliseconds, period: GameTickMilliseconds);

        Chat.MessageAll($"&SFPS was started on &T{mapName}&S.");
    }
    
    internal void Stop()
    {
        Dictionary<string, Player> playersCopy = new Dictionary<string, Player>(Players);

        foreach (Player p in playersCopy.Values)
        {
            PlayerLeftGame(p);
        }

        UnHookEventHandlers();
        IsRunning = false;

        // TODO: Remove animations
        WeaponAnimsHandler.Deactivate();
        WeaponHandler.Deactivate();
        PlayerDataHandler.Instance.Deactivate();

        RemoveServerNameSuffix();
        _mainLoopTimer.Dispose();
        Chat.MessageAll($"&SFPS was stopped.");
    }

    internal void MainLoop(object state)
    {
        if (Monitor.TryEnter(_mainLoopLocker, GameTickMilliseconds))
        {
            try
            {
                _gameState.Loop();
            }
            finally
            {
                Monitor.Exit(_mainLoopLocker);
            }
        }
    }

    internal MapData GetMapData()
    {
        return _databaseManager.GetMapData(Map.name) ?? MapData.Default(Map.name, _gameProperties);
    }

    internal void AppendMapToServerName()
    {
        Server.Config.Name = _defaultServerName + $" (map: {Map.name})";
    }

    internal void RemoveServerNameSuffix()
    {
        Server.Config.Name = _defaultServerName;
    }

    internal bool CanShoot(Player player)
    {
        return (IsShootingEnabled && IsInGame(player));
    }

    internal bool IsInGame(Player player)
    {
        return Players.ContainsValue(player);
    }

    internal int GetCountdownDurationSeconds()
    {
        if (MapData is null || MapData.CountdownTimeSeconds is null)
        {
            return (int)_gameProperties.CountdownDurationSeconds;
        }

        return (int)MapData.CountdownTimeSeconds;
    }

    internal void EndGame()
    {
        _gameState.EndGame();
    }

    internal void SendBindings(Player p)
    {
        // TODO: At some point you want to add a method to retrieve pre-defined bindings
        // that the player can choose via some command. Use a configuration object to store it

        p.Send(Packet.TextHotKey("shootRocket", "/FPSMOShootRocket\n", 35, 0, p.hasCP437)); // Keycode "h"
        p.Send(Packet.TextHotKey("shootGun", "/FPSMOShootGun\n", 36, 0, p.hasCP437)); // Keycode "j"

        p.Send(Packet.TextHotKey("weaponSpeedMinus", "/FPSMOWeaponSpeed minus\n", 37, 0, p.hasCP437)); // Keycode "k"
        p.Send(Packet.TextHotKey("weaponSpeedPlus", "/FPSMOWeaponSpeed plus\n", 38, 0, p.hasCP437)); // Keycode "l"
    }

    internal void RemoveBindings(Player p)
    {
        p.Send(Packet.TextHotKey("shootRocket", "", 35, 0, p.hasCP437)); // Keycode "h"
        p.Send(Packet.TextHotKey("shootGun", "", 36, 0, p.hasCP437)); // Keycode "j"

        p.Send(Packet.TextHotKey("weaponSpeedMinus", "/FPSMOWeaponSpeed minus\n", 37, 0, p.hasCP437)); // Keycode "k"
        p.Send(Packet.TextHotKey("weaponSpeedPlus", "/FPSMOWeaponSpeed plus\n", 38, 0, p.hasCP437)); // Keycode "l"
    }
}