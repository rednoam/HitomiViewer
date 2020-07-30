using ExtensionMethods;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hitomi = HitomiViewer.Hitomi;

namespace HitomiViewer.Scripts.Loaders
{
    class HiyobiLoader
    {
        public readonly Hitomi.Type type = HitomiViewer.Hitomi.Type.Hiyobi;

        public string text;
        public int index;
        public Action<Hitomi, int, int> update = null;
        public Action<int> start = null;
        public Action end = null;

        public HiyobiLoader(string text = null, int? index = null, Action<Hitomi, int, int> update = null, Action<int> start = null, Action end = null)
        {
            this.text = text ?? this.text;
            this.index = index ?? this.index;
            this.update = update ?? this.update;
            this.start = start ?? this.start;
            this.end = end ?? this.end;
        }

        public async void Parser(JObject jobject)
        {
            start(jobject["list"].Count());
            foreach (JToken tk in jobject["list"])
            {
                InternetP parser = new InternetP(url: $"https://cdn.hiyobi.me/data/json/{tk["id"]}_list.json");
                JArray imgs = await parser.TryLoadJArray();
                if (imgs == null) continue;
                Hitomi h = new Hitomi
                {
                    id = tk["id"].ToString(),
                    name = tk["title"].ToString(),
                    type = type,
                    page = imgs.Count,
                    dir = $"https://hiyobi.me/reader/{tk["id"]}",
                    thumb = ImageProcessor.LoadWebImage($"https://cdn.hiyobi.me/tn/{tk["id"]}.jpg"),
                    thumbpath = $"https://cdn.hiyobi.me/tn/{tk["id"]}.jpg",
                    files = imgs.ToList().Select(x => $"https://cdn.hiyobi.me/data/{tk["id"]}/{x["name"]}").ToArray()
                };
                foreach (JToken tags in tk["tags"])
                {
                    HitomiPanel.HitomiInfo.Tag tag = new HitomiPanel.HitomiInfo.Tag();
                    if (tags["value"].ToString().Contains(":"))
                        tag.types = (HitomiViewer.Tag.Types)Enum.Parse(typeof(HitomiViewer.Tag.Types), tags["value"].ToString().Split(':')[0]);
                    else
                        tag.types = HitomiViewer.Tag.Types.tag;
                    tag.name = tags["display"].ToString();
                    h.tags.Add(tag);
                }
                update(h,
                    jobject["list"].ToList().IndexOf(tk),
                    jobject["list"].Count());
            }
            end();
        }
        public async Task<Hitomi> Parser()
        {
            InternetP parser;
            parser = new InternetP(url: $"https://api.hiyobi.me/gallery/{text}");
            JObject obj = await parser.LoadJObject();
            parser = new InternetP(url: $"https://cdn.hiyobi.me/data/json/{text}_list.json");
            JArray imgs = await parser.TryLoadJArray();
            if (imgs == null) return null;
            Hitomi h = new Hitomi
            {
                id = obj["id"].ToString(),
                name = obj["title"].ToString(),
                type = type,
                page = imgs.Count,
                dir = $"https://hiyobi.me/reader/{text}",
                thumb = ImageProcessor.LoadWebImage($"https://cdn.hiyobi.me/tn/{text}.jpg"),
                thumbpath = $"https://cdn.hiyobi.me/tn/{text}.jpg",
                files = imgs.ToList().Select(x => $"https://cdn.hiyobi.me/data/{text}/{x["name"]}").ToArray()
            };
            foreach (JToken tags in obj["tags"])
            {
                HitomiPanel.HitomiInfo.Tag tag = new HitomiPanel.HitomiInfo.Tag();
                if (tags["value"].ToString().Contains(":"))
                    tag.types = (HitomiViewer.Tag.Types)Enum.Parse(typeof(HitomiViewer.Tag.Types), tags["value"].ToString().Split(':')[0]);
                else
                    tag.types = HitomiViewer.Tag.Types.tag;
                tag.name = tags["display"].ToString();
                h.tags.Add(tag);
            }
            return h;
        }
        public void Search()
        {
            InternetP parser = new InternetP(keyword: text.Split(' ').ToList(), index: index);
            parser.HiyobiSearch(data => new InternetP(data: data).ParseJObject(Parser));
        }
    }
    class HitomiLoader
    {
        public int index = 0;
        public int count = 0;
        public Action<Hitomi, int, int> update = null;
        public Action<int> start = null;
        public Action end = null;
        public async void Parser()
        {
            InternetP parser = new InternetP();
            parser.index = (index - 1) * count;
            parser.count = count;
            parser.url = "https://ltn.hitomi.la/index-all.nozomi";
            int[] ids = parser.ByteArrayToIntArray(await parser.LoadNozomi());
            start(ids.Count());
            foreach (int id in ids)
            {
                parser.url = $"https://ltn.hitomi.la/galleryblock/{id}.html";
                parser.index = id;
                Hitomi h = await parser.HitomiData();
                parser.url = $"https://ltn.hitomi.la/galleries/{id}.js";
                JObject info = await parser.HitomiGalleryInfo();
                h.type = Hitomi.Type.Hitomi;
                h.tags = parser.HitomiTags(info);
                h.files = parser.HitomiFiles(info).ToArray();
                h.page = h.files.Length;
                h.thumb = ImageProcessor.LoadWebImage("https:" + h.thumbpath);
                update(h, ids.ToList().IndexOf(id), ids.Count());
            }
            end();
        }
    }
}
