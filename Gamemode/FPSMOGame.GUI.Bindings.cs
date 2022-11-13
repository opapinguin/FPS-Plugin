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

using MCGalaxy;
using MCGalaxy.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPSMO
{
    public sealed partial class FPSMOGame
    {
        public void SendBindings(Player p)
        {
            // TODO: At some point you want to add a method to retrieve pre-defined bindings
            // that the player can choose via some command. Use a configuration object to store it

            p.Send(Packet.TextHotKey("shootRocket", "/FPSMOShootRocket\n", 35, 0, p.hasCP437)); // Keycode "h"
            p.Send(Packet.TextHotKey("shootGun", "/FPSMOShootGun\n", 36, 0, p.hasCP437)); // Keycode "j"

            p.Send(Packet.TextHotKey("weaponSpeedMinus", "/FPSMOWeaponSpeed minus\n", 37, 0, p.hasCP437)); // Keycode "k"
            p.Send(Packet.TextHotKey("weaponSpeedPlus", "/FPSMOWeaponSpeed plus\n", 38, 0, p.hasCP437)); // Keycode "l"
        }

        public void RemoveBindings(Player p)
        {
            p.Send(Packet.TextHotKey("shootRocket", "", 35, 0, p.hasCP437)); // Keycode "h"
            p.Send(Packet.TextHotKey("shootGun", "", 36, 0, p.hasCP437)); // Keycode "j"

            p.Send(Packet.TextHotKey("weaponSpeedMinus", "/FPSMOWeaponSpeed minus\n", 37, 0, p.hasCP437)); // Keycode "k"
            p.Send(Packet.TextHotKey("weaponSpeedPlus", "/FPSMOWeaponSpeed plus\n", 38, 0, p.hasCP437)); // Keycode "l"
        }
    }
}
