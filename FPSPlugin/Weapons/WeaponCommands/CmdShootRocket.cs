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

using FPS.Entities;
using MCGalaxy;

namespace FPS.Weapons
{
    class CmdShootRocket : Command2
    {
        public override string name { get { return "FPSMOShootRocket"; } }
        public override bool LogUsage { get { return false; } }
        public override string type { get { return CommandTypes.Games; } }

        public override void Use(Player player, string message, CommandData data)
        {
            FPSGame game = FPSGame.Instance;

            if (!game.CanShoot(player))
                return;

            PlayerData playerData = PlayerDataHandler.Instance[player.truename];
            Weapon rocket = playerData.rocket;
            playerData.currentWeapon = rocket;

            if (rocket.GetStatus(WeaponHandler.Tick) < 10)
            {
                return;
            }

            rocket.Use(player.Rot, player.Pos.ToVec3F32());
        }

        public override void Help(Player p)
        {
            p.Message("&HShoots a rocket");
        }
    }
}
