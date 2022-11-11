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
using System.Collections.Generic;

namespace FPSMO.Commands
{
    class CmdQueue : Command2
    {
        public override string name { get { return "FPSMOQueue"; } }
        public override string shortcut { get { return "q"; } }
        public override string type { get { return CommandTypes.Games; } }

        public override void Use(Player p, string message, CommandData data)
        {
            List<string> levels = LevelPicker.GetQueue();
            p.Message(levels.ToString());
        }

        public override void Help(Player p)
        {
            p.Message("&HShows the level queue");
        }
    }
}
