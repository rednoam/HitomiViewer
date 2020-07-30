using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Scripts
{
    class Config
    {
        public readonly string path = Path.Combine(MainWindow.rootDir, "config.json");
        private JObject config;

        public JObject Load()
        {
            if (!File.Exists(path))
                return null;
            config = JObject.Parse(File.ReadAllText(path));
            return config;
        }

        public bool? BoolValue(string path)
        {
            if (!config.ContainsKey(path)) return null;
            return bool.Parse(config[path].ToString());
        }
        public IList<T> ArrayValue<T>(string path) where T : class
        {
            if (!config.ContainsKey(path)) return new List<T>();
            return config[path].ToObject<List<T>>();
        }

        public bool Save(JObject data)
        {
            File.WriteAllText(path, data.ToString());
            return true;
        }
    }
}
