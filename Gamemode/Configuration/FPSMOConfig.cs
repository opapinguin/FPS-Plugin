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

using System.IO;
using System.Xml.Serialization;

namespace FPSMO.Configuration
{
    /// <summary>
    /// A simple CRUD API for configuration data
    /// </summary>
    public static class FPSMOConfig<T> where T : struct
    {
        public static string dir;

        public static void Create(string key, T obj)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(dir + "/" + key + ".xml"))
            {
                Update(key, obj);
            }
        }

        public static T Read(string key)
        {
            T result;
            XmlSerializer reader = new XmlSerializer(typeof(T));
            using (var sr = new StreamReader(dir + "/" + key + ".xml"))
            {
                result = (T)reader.Deserialize(sr);
            }
            return result;
        }

        public static void Update(string key, T obj)
        {
            XmlSerializer writer = new XmlSerializer(typeof(T));
            using (StreamWriter sw = new StreamWriter(dir + "/" + key + ".xml"))
            {
                writer.Serialize(sw, obj);
            }
            return;
        }

        public static void Delete(string key)
        {
            if (File.Exists(dir + "/" + key + ".xml"))
            {
                File.Delete(dir + "/" + key + ".xml");
            }
        }
    }
}
