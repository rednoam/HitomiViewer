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
                return new JObject();
            config = JObject.Parse(File.ReadAllText(path));
            return config;
        }

        public string StringValue(string path)
        {
            if (config == null) return null;
            if(!config.ContainsKey(path)) return null;
            return config[path].ToString();
        }
        public bool? BoolValue(string path)
        {
            if (config == null) return null;
            if (!config.ContainsKey(path)) return null;
            return bool.Parse(config[path].ToString());
        }
        public IList<T> ArrayValue<T>(string path) where T : class
        {
            if (config == null) return new List<T>();
            if (!config.ContainsKey(path)) return new List<T>();
            return config[path].ToObject<List<T>>();
        }

        public bool Save(JObject data)
        {
            this.config = data;
            Global.cfg = this;
            Global.cfgob = data;
            Global.Password = StringValue("pw");
            Global.DownloadFolder = StringValue("df") ?? "hitomi_downloaded";
            Global.FileEn = BoolValue("fe") ?? false;
            Global.AutoFileEn = BoolValue("autofe") ?? false;
            Global.EncryptTitle = BoolValue("et") ?? false;
            Global.RandomTitle = BoolValue("rt") ?? false;
            if (Global.DownloadFolder == "") Global.DownloadFolder = "hitomi_downloaded";
            File.WriteAllText(path, data.ToString());
            return true;
        }
    }
}
