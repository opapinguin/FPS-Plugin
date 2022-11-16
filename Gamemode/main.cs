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
using FPSMO;
using MCGalaxy.Events.PlayerEvents;
using System;
using MCGalaxy.Network;
using MCGalaxy.SQL;
using FPSMO.Commands;
using FPSMO.Weapons;
using FPSMO.DB;
using FPSMO.Teams;

namespace MCGalaxy
{
    internal class FPSMO : Plugin
    {
        FPSMOGame game = FPSMOGame.Instance;

        public override string creator { get { return "Opapinguin, D_Flat, Razorboot, Panda"; } }

        public override string name { get { return "FPSMO"; } }

        public override string MCGalaxy_Version { get { return "1.9.4.0"; } }

        public override void Load(bool startup)
        {
            DatabaseHandler.InitializeDatabase();

            var achievementsManager = new AchievementsManager(game);
            var gui = new GUI(game, achievementsManager);
            /***********
             * COMMANDS *
             ************/
            #region Commands
            Command.Register(new CmdAchievements(achievementsManager));
            Command.Register(new CmdAchievementTest(achievementsManager));

            Command.Register(new CmdSwapTeam());

            Command.Register(new CmdFPSMO());
            Command.Register(new CmdQueue());
            Command.Register(new CmdRate());

            Command.Register(new CmdShootGun());
            Command.Register(new CmdShootRocket());

            Command.Register(new CmdWeaponSpeed());

            #endregion Commands

            game.Start();   // TODO: Make autostart optional?
        }

        public override void Unload(bool shutdown)
        {
            Command.Unregister(Command.Find("FPSMOSwapTeam"));

            Command.Unregister(Command.Find("FPSMO"));
            Command.Unregister(Command.Find("FPSMOQueue"));
            Command.Unregister(Command.Find("FPSMORate"));

            Command.Unregister(Command.Find("FPSMOShootGun"));
            Command.Unregister(Command.Find("FPSMOShootRocket"));

            Command.Unregister(Command.Find("FPSMOWeaponSpeed"));

            Command.Unregister(Command.Find("AchievementTest"));
            Command.Unregister(Command.Find("Achievements"));

            game.Stop();
        }
    }

}
