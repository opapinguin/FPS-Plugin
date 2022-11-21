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
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace FPSMO.Configuration
{
    /// <summary>
    /// A simple CRUD API for configuration data
    /// Basically datalists but allowing complex objects to be stored
    /// </summary>
    internal static class FPSMOConfig<T> where T : class, new()
    {
        internal static string dir;
        internal static ParserContext parserContext;

        internal static void Create(string key, T obj)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(dir + "/" + key + ".txt"))
            {
                Update(key, obj);
            }
        }

        internal static T Read(string key)
        {
            parserContext = new ParserContext();

            T result = new T();

            string[] lines = File.ReadAllLines(dir + "/" + key + ".txt");

            var type = typeof(T);
            PropertyInfo[] props = type.GetProperties();

            // Foreach property in T
            foreach (var prop in props)
            {
                // Compare with each string till you find the name of the property
                foreach (string l in lines)
                {
                    int idx = l.IndexOf(':');
                    if (l.Substring(0, idx) == prop.Name)
                    {
                        // In here check against every possible type that the property could have

                        string value = l.Substring(idx + 1);
                        PropertyInfo propertyInfo = type.GetProperty(prop.Name);
                        object boxed = result;

                        if (prop.PropertyType == typeof(int))
                        {
                            parserContext.SetStrategy(new IntParser());
                            propertyInfo.SetValue(boxed, (int)parserContext.FromString(value), null);
                        } else if (prop.PropertyType == typeof(uint))
                        {
                            parserContext.SetStrategy(new UIntParser());
                            propertyInfo.SetValue(boxed, (uint)parserContext.FromString(value), null);
                        } else if (prop.PropertyType == typeof(float))
                        {
                            parserContext.SetStrategy(new FloatParser());
                            propertyInfo.SetValue(boxed, (float)parserContext.FromString(value), null);
                        } else if (prop.PropertyType == typeof(List<string>))
                        {
                            parserContext.SetStrategy(new StringListParser());
                            propertyInfo.SetValue(boxed, (List<string>)parserContext.FromString(value), null);
                        }
                        else if (prop.PropertyType == typeof(Boolean))
                        {
                            parserContext.SetStrategy(new BooleanParser());
                            propertyInfo.SetValue(boxed, (bool)parserContext.FromString(value), null);
                        }
                        else if (prop.PropertyType == typeof(System.UInt16))
                        {
                            parserContext.SetStrategy(new UInt16Parser());
                            propertyInfo.SetValue(boxed, (System.UInt16)parserContext.FromString(value), null);
                        }

                        result = (T)boxed;
                    }
                }
            }

            return result;
        }
        
        internal static void Update(string key, T obj)
        {
            parserContext = new ParserContext();

            var sb = new StringBuilder();

            var type = typeof(T);
            PropertyInfo[] props = type.GetProperties();

            // Foreach property in T
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(int))
                {
                    parserContext.SetStrategy(new IntParser());
                }
                else if (prop.PropertyType == typeof(uint))
                {
                    parserContext.SetStrategy(new UIntParser());
                }
                else if (prop.PropertyType == typeof(float))
                {
                    parserContext.SetStrategy(new FloatParser());
                }
                else if (prop.PropertyType == typeof(List<string>))
                {
                    parserContext.SetStrategy(new StringListParser());
                }
                else if (prop.PropertyType == typeof(Boolean))
                {
                    parserContext.SetStrategy(new BooleanParser());
                }
                else if (prop.PropertyType == typeof(System.UInt16))
                {
                    parserContext.SetStrategy(new UInt16Parser());
                }

                Logger.Log(LogType.ConsoleMessage, prop.Name);
                Logger.Log(LogType.ConsoleMessage, prop.GetValue(obj, null).ToString());

                string propAsString = parserContext.ToString(prop.GetValue(obj, null));
                sb.Append(prop.Name + ":" + propAsString);
                sb.AppendLine();
            }

            string fileContent = sb.ToString();

            File.WriteAllText(dir + "/" + key + ".txt", fileContent);
        }

        internal static void Delete(string key)
        {
            if (File.Exists(dir + "/" + key + ".txt"))
            {
                File.Delete(dir + "/" + key + ".txt");
            }
        }
    }
}
