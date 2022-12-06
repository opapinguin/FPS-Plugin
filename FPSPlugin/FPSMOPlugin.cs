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
using System.IO;
using FPS;
using FPS.Commands;
using FPS.Configuration;
using FPS.DB;
using FPS.Weapons;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Games;

namespace MCGalaxy;

internal class FPSMOPlugin : Plugin
{
    private FPSGame _game;
    private GUI _gui;
    private AchievementsManager _achievementsManager;
    private DatabaseManager _databaseManager;
    private GameProperties _gameProperties;
    private FPS.LevelPicker _levelPicker;

    public override string creator { get { return "Opapinguin, D_Flat, Razorboot, Panda"; } }
    public override string name { get { return "FPSMO"; } }
    public override string MCGalaxy_Version { get { return "1.9.4.4"; } }

    internal event EventHandler PluginLoaded;
    private void OnPluginLoaded()
    {
        if (PluginLoaded != null) PluginLoaded(this, EventArgs.Empty);
    }

    internal event EventHandler PluginUnloading;
    private void OnPluginUnloading()
    {
        if (PluginUnloading != null) PluginUnloading(this, EventArgs.Empty);
    }

    public override void Load(bool startup)
    {
        _game = FPSGame.Instance;
        _achievementsManager = LoadAchievementsManager(_game);
        _databaseManager = LoadDatabaseManager(_achievementsManager);
        _gui = LoadGUI(_game, _achievementsManager);
        _game.SetDatabaseManager(_databaseManager);

        LoadGameProperties();

        _levelPicker = new FPS.LevelPicker(_databaseManager, (int)_gameProperties.MapHistory);
        _game.LevelPicker = _levelPicker;

        UnloadVanillaCommands();
        RegisterCommands();

        if (_gameProperties.AutoStart)
            StartGame();

        OnPluginLoaded();
    }

    public override void Unload(bool shutdown)
    {
        OnPluginUnloading();

        UnregisterCommands();
        ReloadVanillaCommands();
        StopGame();
        UnloadAchievementsManager();
        UnloadDatabaseManager();
        UnloadGUI();
    }

    private AchievementsManager LoadAchievementsManager(FPSGame game)
    {
        var achievementsManager = new AchievementsManager();
        achievementsManager.Observe(game);

        return achievementsManager;
    }

    private void UnloadAchievementsManager()
    {
        _achievementsManager.Unobserve(_game);
    }

    private DatabaseManager LoadDatabaseManager(AchievementsManager achievementsManager)
    {
        var databaseManager = new DatabaseManager();
        databaseManager.CreateTables(checkExistence: true);
        databaseManager.Observe(_achievementsManager);
        databaseManager.ObserveMCGalaxy();

        return databaseManager;
    }

    private void UnloadDatabaseManager()
    {
        _databaseManager.Unobserve(_achievementsManager);
        _databaseManager.UnobserveMCGalaxy();
    }

    private GUI LoadGUI(FPSGame game, AchievementsManager achievementsManager)
    {
        var gui = new GUI(_game);
        gui.Observe(this);
        gui.Observe(_game);
        gui.Observe(_achievementsManager);
        gui.ObserveJoinedLevel();

        return gui;
    }

    private void UnloadGUI()
    {
        _gui.Unobserve(this);
        _gui.Unobserve(_game);
        _gui.Unobserve(_achievementsManager);
        _gui.UnobserveJoinedLevel();
    }

    private void StartGame()
    {
        _game.Start();
    }

    private void StopGame()
    {
        _game.Stop();
    }

    private void RegisterCommands()
    {
        Command.Register(new CmdAchievements(_achievementsManager));
        Command.Register(new CmdAchievementTest(_achievementsManager));
        Command.Register(new CmdSwapTeam());
        Command.Register(new CmdFPS(_game, _databaseManager));
        Command.Register(new CmdVoteQueue(_databaseManager, _levelPicker));
        Command.Register(new CmdQueue(_databaseManager, _levelPicker));
        Command.Register(new CmdRate(_databaseManager));
        Command.Register(new CmdShootGun());
        Command.Register(new CmdShootRocket());
        Command.Register(new CmdWeaponSpeed());
    }

    private void UnregisterCommands()
    {
        Command.Unregister(Command.Find("FPSMOSwapTeam"));
        Command.Unregister(Command.Find("FPSMO"));
        Command.Unregister(Command.Find("VoteQueue"));
        Command.Unregister(Command.Find("Queue"));
        Command.Unregister(Command.Find("Rate"));
        Command.Unregister(Command.Find("FPSMOShootGun"));
        Command.Unregister(Command.Find("FPSMOShootRocket"));
        Command.Unregister(Command.Find("FPSMOWeaponSpeed"));
        Command.Unregister(Command.Find("AchievementTest"));
        Command.Unregister(Command.Find("Achievements"));
    }

    private void UnloadVanillaCommands()
    {
        Command.Unregister(Command.Find("Queue"));
    }

    private void ReloadVanillaCommands()
    {
        Command.Register(new MCGalaxy.Commands.Fun.CmdQueue());
    }

    private void LoadGameProperties()
    {
        if (!Directory.Exists(Constants.FPS_DIRECTORY_PATH))
        {
            Directory.CreateDirectory(Constants.FPS_DIRECTORY_PATH);
        }

        if (!File.Exists(Constants.GAME_PROPERTIES_FILE_PATH))
        {
            GameProperties.Save(GameProperties.Default(), Constants.FPS_DIRECTORY_PATH);
        }

        _gameProperties = GameProperties.Load(Constants.GAME_PROPERTIES_FILE_PATH);
        _game.SetGameProperties(_gameProperties);
    }
}
