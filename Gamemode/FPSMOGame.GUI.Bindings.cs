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

            p.Send(Packet.TextHotKey("shootGun", "/FPSMOShootGun\n", 36, 0, p.hasCP437)); // Keycode "j"
        }

        public void RemoveBindings(Player p)
        {
            p.Send(Packet.TextHotKey("shootGun", "", 36, 0, p.hasCP437)); // Keycode "j"
        }
    }
}
