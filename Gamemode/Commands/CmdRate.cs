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

using FPSMO.Configuration;
using MCGalaxy;
using System.IO;

namespace FPSMO.Commands
{
    public class CmdRate : Command2
    {
        public override string name { get { return "FPSMORate"; } }
        public override string shortcut { get { return "Rate"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data)
        {
            int rating;
            int oldRating = int.MaxValue;
            if (message == "")
            {
                Help(p); return;
            }

            if (!int.TryParse(message, out rating))
            {
                p.Message("Not a valid input");
                return;
            }

            if (rating > 5 || rating < 1)
            {
                p.Message("Not a valid input");
                return;
            }

            string path = "FPSMO/Ratings";

            if (CheckIsAuthor(p))
            {
                p.Message("Cannot rate this map as you are an author of it"); return;
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            PlayerExtList levelList = PlayerExtList.Load(path + "/" + p.level.name + ".txt"); // Automatically creates the file as well

            if (levelList.Contains(p.truename))
            {
                oldRating = int.Parse(levelList.FindData(p.truename));
            }

            levelList.Update(p.truename, rating.ToString());

            FPSMOMapConfig config = FPSMOGame.Instance.mapConfig;

            if (oldRating == int.MaxValue)
            {
                config.SUM_RATINGS += rating;
                config.TOTAL_RATINGS += 1;

                p.SetMoney(p.money + 5);
                p.Message("Thank you for voting! You received 5 " + Server.Config.Currency);
            }
            else
            {
                config.SUM_RATINGS -= oldRating;
                config.SUM_RATINGS += rating;

                p.Message("Thank you for rating this map!");
            }

            // Update configuration
            FPSMOConfig<FPSMOMapConfig>.Update(FPSMOGame.Instance.map.name, config);
            FPSMOGame.Instance.mapConfig = config;

            levelList.Save();
            p.level.SaveSettings();
        }
        protected static bool CheckIsAuthor(Player p)
        {
            string[] authors = p.level.Config.Authors.SplitComma();
            return authors.CaselessContains(p.truename);
        }

        public override void Help(Player p)
        {
            p.Message("&T/Rate [num]");
            p.Message("&HRates a map from 1 to 5");
        }
    }
}
