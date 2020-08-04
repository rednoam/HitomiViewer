using ExtensionMethods;
using HitomiViewer.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Scripts
{
    class HiyobiTags
    {
        public static readonly string path = Path.Combine(MainWindow.rootDir, "tagdata.json");
        public static List<Tag> Tags = null;

        public static async void LoadTags()
        {
            if (!File.Exists(path))
            {
                InternetP parser = new InternetP();
                JArray tags = await parser.HiyobiTags();
                Tags = tags.Select(x => new Tag { name = x.ToString(), types = Tag.ParseTypes(x.ToString()) }).ToList();
                File.WriteAllText(path, tags.ToString());
            }
            else
            {
                JArray tags = JArray.Parse(File.ReadAllText(path));
                Tags = tags.Select(x => new Tag { name = x.ToString(), types = Tag.ParseTypes(x.ToString()) }).ToList();
            }
        }
    }
}
