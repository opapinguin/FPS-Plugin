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
using FPSMO;
using FPSMO.Commands;
using FPSMO.DB;
using FPSMO.Weapons;

namespace MCGalaxy
{
    internal class FPSMOPlugin : Plugin
    {
        private FPSMOGame _game;
        private GUI _gui;
        private AchievementsManager _achievementsManager;

        public override string creator { get { return "Opapinguin, D_Flat, Razorboot, Panda"; } }
        public override string name { get { return "FPSMO"; } }
        public override string MCGalaxy_Version { get { return "1.9.4.0"; } }

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
            _game = FPSMOGame.Instance;
            _achievementsManager = new AchievementsManager(_game);

            InitDatabase();
            InitGUI();
            RegisterCommands();

            _game.Start();
            OnPluginLoaded();
        }

        public override void Unload(bool shutdown)
        {
            OnPluginUnloading();
            UnregisterCommands();
            DatabaseHandler.UnsubscribeFrom(_achievementsManager);
            _game.Stop();
            _gui.UnsubscribeFromAll(this, _game, _achievementsManager);
        }

        private void InitGUI()
        {
            _gui = new GUI(this, _game, _achievementsManager);
        }

        private void InitDatabase()
        {
            DatabaseHandler.InitializeDatabase();
            DatabaseHandler.SubscribeTo(_achievementsManager);
        }

        private void RegisterCommands()
        {
            Command.Register(new CmdAchievements(_achievementsManager));
            Command.Register(new CmdAchievementTest(_achievementsManager));
            Command.Register(new CmdSwapTeam());
            Command.Register(new CmdFPS(_game));
            Command.Register(new CmdVoteQueue());
            Command.Register(new CmdRate());
            Command.Register(new CmdShootGun());
            Command.Register(new CmdShootRocket());
            Command.Register(new CmdWeaponSpeed());
        }

        private void UnregisterCommands()
        {
            Command.Unregister(Command.Find("FPSMOSwapTeam"));
            Command.Unregister(Command.Find("FPSMO"));
            Command.Unregister(Command.Find("VoteQueue"));
            Command.Unregister(Command.Find("FPSMORate"));
            Command.Unregister(Command.Find("FPSMOShootGun"));
            Command.Unregister(Command.Find("FPSMOShootRocket"));
            Command.Unregister(Command.Find("FPSMOWeaponSpeed"));
            Command.Unregister(Command.Find("AchievementTest"));
            Command.Unregister(Command.Find("Achievements"));
        }
    }

}
