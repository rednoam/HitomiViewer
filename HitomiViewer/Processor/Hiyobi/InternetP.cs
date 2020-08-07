using ExtensionMethods;
using HitomiViewer.Scripts;
using HitomiViewer.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Processor
{
    partial class InternetP
    {
        public async void HiyobiSearch(Action<string> callback) => callback(await HiyobiSearch());
        public async Task<List<Hitomi>> HiyobiList()
        {
            List<Hitomi> output = new List<Hitomi>();
            url = $"https://api.hiyobi.me/list/{index}";
            JObject obj = await LoadJObject();
            foreach (JToken item in obj["list"])
            {
                Hitomi h = HiyobiParse(item);
                h.type = Hitomi.Type.Hiyobi;
                output.Add(h);
            }
            return output;
        }
        public async Task<List<HitomiFile>> HiyobiFiles()
        {
            List<HitomiFile> files = new List<HitomiFile>();
            url = $"https://cdn.hiyobi.me/data/json/{index}_list.json";
            JArray arr = await LoadJArray();
            foreach (JToken tk in arr)
            {
                files.Add(new HitomiFile
                {
                    hasavif = (tk.IntValue("hasavif") ?? 0).ToBool(),
                    hash = tk.StringValue("hash"),
                    haswebp = (tk.IntValue("haswebp") ?? 0).ToBool(),
                    height = tk.IntValue("height") ?? 0,
                    width = tk.IntValue("width") ?? 0,
                    name = tk.StringValue("name"),
                    url = $"https://cdn.hiyobi.me/data/{index}/{tk.StringValue("name")}"
                });
            }
            return files;
        }
        public async Task<Hitomi> HiyobiDataNumber(int? index = null)
        {
            url = $"https://api.hiyobi.me/gallery/{index ?? this.index}";
            JObject obj = await LoadJObject();
            Hitomi h = HiyobiParse(obj);
            h.type = Hitomi.Type.Hiyobi;
            return h;
        }
        public async Task<string> HiyobiSearch()
        {
            HttpClient client = new HttpClient();
            List<KeyValuePair<string, string>> body = new List<KeyValuePair<string, string>>();
            foreach (var key in this.keyword)
            {
                body.Add(new KeyValuePair<string, string>("search[]", key));
            }
            body.Add(new KeyValuePair<string, string>("paging", this.index.ToString()));
            var response = await client.PostAsync("https://api.hiyobi.me/search", new FormUrlEncodedContent(body));
            var pageContents = await response.Content.ReadAsStringAsync();
            return pageContents;
        }
        public Hitomi HiyobiParse(JToken item)
        {
            Hitomi h = new Hitomi();
            h.authors = item["artists"].Select(x => x.StringValue("display")).ToArray();
            h.id = item.StringValue("id");
            h.language = item.StringValue("language");
            h.tags = item["tags"].Select(x => new Tag { name = x.StringValue("display"), types = Tag.ParseTypes(x.StringValue("value")) }).ToList();
            h.name = item.StringValue("title");
            h.designType = DesignTypeFromString(item.StringValue("type"));
            h.thumbpath = $"https://cdn.hiyobi.me/tn/{h.id}.jpg";
            h.thumb = ImageProcessor.LoadWebImage(h.thumbpath);
            h.dir = $"https://hiyobi.me/reader/{h.id}";
            h.page = 0;
            h.AutoAuthor();
            return h;
        }
        public async Task<JArray> HiyobiTags()
        {
            url = "https://api.hiyobi.me/auto.json";
            return await LoadJArray();
        }
        /// <summary>
        /// Hiyobi 에서 불러올 수 있는지와 불러올 수 있다면 Hitomi 데이터까지 반환합니다.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<Tuple<bool, Hitomi>> isHiyobiData(int? index = null)
        {
            try
            {
                Hitomi h = await HiyobiDataNumber(index);
                return new Tuple<bool, Hitomi>(true, h);
            }
            catch { return new Tuple<bool, Hitomi>(false, null); }
        }
        public async Task<bool> isHiyobi(int? index = null)
        {
            try
            {
                _ = await Load($"https://cdn.hiyobi.me/data/json/{index ?? this.index}.json");
                return true;
            }
            catch { return false; }
        }
    }
}
