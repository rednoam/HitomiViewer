using Newtonsoft.Json.Linq;
using System;
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

        public JObject Load()
        {
            if (!File.Exists(path))
                return null;
            return JObject.Parse(File.ReadAllText(path));
        }

        public bool Save(JObject data)
        {
            File.WriteAllText(path, data.ToString());
            return true;
        }
    }
}
