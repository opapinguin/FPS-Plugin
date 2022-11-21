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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

/// <summary>
/// This file implements different readers/writers for various types using the strategy pattern.
/// </summary>
namespace FPSMO.Configuration
{
    /// <summary>
    /// The strategy interface
    /// </summary>
    internal interface IParserStrategy
    {
        object FromString(string str);
        string ToString(object value);
    }

    internal class ParserContext
    {
        private IParserStrategy strategy;

        public void SetStrategy(IParserStrategy strategy)
        {
            this.strategy = strategy;
        }

        public object FromString(string str)
        {
            return strategy.FromString(str);
        }
        
        public string ToString(object value)
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

    internal class UIntParser : IParserStrategy {
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
            if (str.ToLower() == "true")
            {
                return true;
            } else
            {
                return false;
            }
        }
        public string ToString(object value)
        {
            if ((bool)value)
            {
                return "true";
            } else
            {
                return "false";
            }
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