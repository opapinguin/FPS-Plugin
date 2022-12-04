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
using System.Linq;
using System.IO;
using BlockID = System.UInt16;
using System.Text;
using System;

namespace FPS.Configuration
{
    internal class GamePropertiesParser
    {
        internal GameProperties Parse(string[] lines)
        {
            ParserContext parserContext = new ParserContext();
            GameProperties properties = new GameProperties();

            foreach (string line in lines)
            {
                ParseLine(line, properties, parserContext);
            }

            return properties;
        }

        private void ParseLine(string line, GameProperties properties, ParserContext parserContext)
        {
            if (IsComment(line)) return;

            string lineNoSpaces = Utils.RemoveSpaces(line);
            string[] parts = lineNoSpaces.Split('=');
            if (parts.Length != 2) return;

            switch (parts[0].ToLower())
            {
                case "autostart":
                    parserContext.SetStrategy(new BooleanParser());
                    properties.AutoStart = (bool)parserContext.FromString(parts[1]);
                    break;
                case "countdown_duration_seconds":
                    parserContext.SetStrategy(new UIntParser());
                    properties.CountdownDurationSeconds = (uint)parserContext.FromString(parts[1]);
                    break;
                case "vote_duration_seconds":
                    parserContext.SetStrategy(new UIntParser());
                    properties.VoteDurationSeconds = (uint)parserContext.FromString(parts[1]);
                    break;
                case "default_round_duration_seconds":
                    parserContext.SetStrategy(new UIntParser());
                    properties.DefaultRoundDurationSeconds = (uint)parserContext.FromString(parts[1]);
                    break;
                case "afk_notice_seconds":
                    parserContext.SetStrategy(new UIntParser());
                    properties.AFKNoticeSeconds = (uint)parserContext.FromString(parts[1]);
                    break;
                case "afk_mode_seconds":
                    parserContext.SetStrategy(new UIntParser());
                    properties.AFKModeSeconds = (uint)parserContext.FromString(parts[1]);
                    break;
                case "map_history":
                    parserContext.SetStrategy(new UIntParser());
                    properties.MapHistory = (uint)parserContext.FromString(parts[1]);
                    break;
                case "team_versus_team_mode":
                    parserContext.SetStrategy(new BooleanParser());
                    properties.TeamVersusTeamMode = (bool)parserContext.FromString(parts[1]);
                    break;
            }
        }

        private bool IsComment(string line)
        {
            return line.StartsWith("#");
        }
    }

    internal interface IParserStrategy
    {
        object FromString(string str);
        string ToString(object value);
    }

    internal class ParserContext
    {
        private IParserStrategy strategy;

        internal void SetStrategy(IParserStrategy strategy)
        {
            this.strategy = strategy;
        }

        internal object FromString(string str)
        {
            return strategy.FromString(str);
        }

        internal string ToString(object value)
        {
            return this.strategy.ToString(value);
        }
    }

    internal class IntParser : IParserStrategy
    {
        public object FromString(string str)
        {
            return int.Parse(str);
        }
        public string ToString(object value)
        {
            return ((int)value).ToString();
        }
    }

    internal class UIntParser : IParserStrategy
    {
        public object FromString(string str)
        {
            return uint.Parse(str);
        }
        public string ToString(object value)
        {
            return ((uint)value).ToString();
        }
    }

    internal class FloatParser : IParserStrategy
    {
        public object FromString(string str)
        {
            return float.Parse(str);
        }
        public string ToString(object value)
        {
            return ((float)value).ToString();
        }
    }

    internal class StringListParser : IParserStrategy
    {
        public object FromString(string str)
        {
            return str.Split(',').ToList();
        }
        public string ToString(object value)
        {
            return string.Join(",", ((List<string>)value).ToArray());
        }
    }

    internal class BooleanParser : IParserStrategy
    {
        public object FromString(string str)
        {
            if (str.ToLower() == "true" || str.ToLower() == "yes" || str == "1")
            {
                return true;
            }
            else if (str.ToLower() == "false" || str.ToLower() == "no" || str == "0")
            {
                return false;
            }

            throw new ArgumentException($"{str} could not be parsed as a boolean.");
        }

        public string ToString(object value)
        {
            bool valueBool = (bool)value;
            return valueBool ? "true" : "false";
        }
    }

    internal class UInt16Parser : IParserStrategy
    {
        public object FromString(string str)
        {
            return System.UInt16.Parse(str);
        }

        public string ToString(object value)
        {
            return ((System.UInt16)value).ToString();
        }
    }
}
