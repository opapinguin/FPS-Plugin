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
using System;

namespace FPSMO.Weapons
{
    class CmdShootRocket : Command2
    {
        public override string name { get { return "FPSMOShootRocket"; } }
        public override bool LogUsage { get { return false; } }
        public override string type { get { return CommandTypes.Games; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (!(FPSMOGame.Instance.stage == FPSMOGame.Stage.Round && FPSMOGame.Instance.subStage == FPSMOGame.SubStage.Middle))
            {
                return;
            }

            if (PlayerDataHandler.Instance[p.truename] == null)
            {
                return;
            }

            Weapon rocket = PlayerDataHandler.Instance[p.truename].rocket;
            if (rocket.GetStatus(WeaponAnimsHandler.Tick) < 10)
            {
                return;
            }
            PlayerDataHandler.Instance[p.truename].rocket.Use(p.Rot, p.Pos.ToVec3F32(), 10);
            PlayerDataHandler.Instance[p.truename].currentWeapon = rocket;
        }

        public override void Help(Player p)
        {
            p.Message("&HShoots a rocket");
        }
    }
}
