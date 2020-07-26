using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Scripts
{
    class InternetP
    {
        public string url { get; set; }
        public string data { get; set; }
        public List<string> keyword { get; set; }
        public int index { get; set; }
        public int count { get; set; }

        public InternetP(string url = null, string data = null, List<string> keyword = null, int? index = null)
        {
            if (url != null) this.url = url;
            if (data != null) this.data = data;
            if (keyword != null) this.keyword = keyword;
            if (index != null) this.index = index.Value;
        }

        public async void LoadJObject(Action<JObject> callback)
        {
            callback(await LoadJObject());
        }
        public async Task<JObject> LoadJObject()
        {
            string html = await Load(url);
            return JObject.Parse(html);
        }
        public async void LoadJArray(Action<JArray> callback)
        {
            callback(await LoadJArray());
        }
        public async Task<JArray> LoadJArray()
        {
            string html = await Load(url);
            return JArray.Parse(html);
        }

        public async void TryLoadJObject(Action<JObject> callback)
        {
            callback(await TryLoadJObject());
        }
        public async Task<JObject> TryLoadJObject()
        {
            try
            {
                string html = await Load(url);
                return JObject.Parse(html);
            }
            catch
            {
                return null;
            }
        }
        public async void TryLoadJArray(Action<JArray> callback)
        {
            callback(await TryLoadJArray());
        }
        public async Task<JArray> TryLoadJArray()
        {
            try
            {
                string html = await Load(url);
                return JArray.Parse(html);
            }
            catch
            {
                return null;
            }
        }

        public void ParseJObject(Action<JObject> callback)
        {
            callback(ParseJObject());
        }
        public JObject ParseJObject()
        {
            return JObject.Parse(data);
        }
        public void ParseJArray(Action<JArray> callback)
        {
            callback(ParseJArray());
        }
        public JArray ParseJArray()
        {
            return JArray.Parse(data);
        }

        public async void HiyobiSearch(Action<string> callback)
        {
            callback(await HiyobiSearch());
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
        public async Task<Hitomi> HitomiData()
        {
            string html = await Load(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            Hitomi h = new Hitomi();
            h.dir = $"https://hitomi.la/reader/{index}.html";
            //h.dir = url;
            HtmlNode name = doc.DocumentNode.SelectSingleNode("//h1[@class=\"lillie\"]");
            h.name = name.InnerText;
            HtmlNode image = doc.DocumentNode.SelectSingleNode("//div[@class=\"dj-img1\"]/img");
            h.thumbpath = image.GetDataAttribute("src").Value;
            HtmlNode table = doc.DocumentNode.SelectSingleNode("//table[@class=\"dj-desc\"]");
            for (var i = 0; i < table.ChildNodes.Count-1; i += 2)
            {
                HtmlNode tr = table.ChildNodes[i+1];
                string trname = tr.ChildNodes[0].InnerHtml;
                string trtext = tr.ChildNodes[tr.ChildNodes.Count / 2].InnerHtml.Trim();
            }
            return h;
        }
        public async Task<JObject> HitomiGalleryInfo()
        {
            string html = await Load(url);
            JObject jObject = JObject.Parse(html.Replace("var galleryinfo = ", ""));
            return jObject;
        }
        public async Task<Hitomi> HitomiGalleryData()
        {
            JObject jObject = await HitomiGalleryInfo();
            List<HitomiFile> files = new List<HitomiFile>();
            foreach (JToken tag1 in jObject["files"])
            {
                files.Add(new HitomiFile
                {
                    hash = tag1["hash"].ToString(),
                    name = tag1["name"].ToString(),
                    hasavif = Convert.ToBoolean(int.Parse(tag1["hasavif"].ToString())),
                    haswebp = Convert.ToBoolean(int.Parse(tag1["haswebp"].ToString()))
                });
            }
            return null;
        }
        public List<HitomiPanel.HitomiInfo.Tag> HitomiTags(JObject jObject)
        {
            List<HitomiPanel.HitomiInfo.Tag> tags = new List<HitomiPanel.HitomiInfo.Tag>();
            foreach (JToken tag1 in jObject["tags"])
            {
                HitomiPanel.HitomiInfo.Tag tag = new HitomiPanel.HitomiInfo.Tag();
                tag.types = Tag.Types.tag;
                if (tag1.SelectToken("female") != null && tag1["female"].ToString() == "1")
                    tag.types = Tag.Types.female;
                if (tag1.SelectToken("male") != null && tag1["male"].ToString() == "1")
                    tag.types = Tag.Types.male;
                tag.name = tag1["tag"].ToString();
                tags.Add(tag);
            }
            return tags;
        }
        public List<string> HitomiFiles(JObject jObject)
        {
            List<string> files = new List<string>();
            foreach (JToken tag1 in jObject["files"])
            {
                files.Add($"https://aa.hitomi.la/webp/{HitomiFullPath(tag1["hash"].ToString())}.webp");
            }
            return files;
        }
        public async Task<byte[]> LoadNozomi()
        {
            if (url.Last() == '/') url = url.Remove(url.Length - 1);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(index * 4, (index + count) * 4 - 1);
            var response = await client.GetAsync(url);
            var pageContents = await response.Content.ReadAsByteArrayAsync();
            return pageContents;
        }
        public async Task<string> Load(string Url)
        {
            if (Url.Last() == '/') Url = Url.Remove(Url.Length - 1);
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(Url);
            var pageContents = await response.Content.ReadAsStringAsync();
            return pageContents;
        }
        public int[] ByteArrayToIntArray(byte[] arr)
        {
            List<int> intarr = new List<int>();
            int co = arr.Length / 4;
            for (var i = 0; i < co; i++)
            {
                byte[] iarr = new byte[4];
                iarr = arr.ToList().Skip(i * 4).Take(4).ToArray();
                intarr.Add(BitConverter.ToInt32(iarr.Reverse().ToArray(), 0));
            }
            return intarr.ToArray();
        }

        public string HitomiFullPath(string str)
        {
            string first = str.Last().ToString();
            string second = string.Join("", str.Substring(str.Length - 3).Take(2));
            return $"{first}/{second}/{str}";
        }

        class HitomiFile
        {
            public string path { get; set; }
            public string hash { get; set; }
            public string name { get; set; }
            public bool hasavif { get; set; }
            public bool haswebp { get; set; }
        }
    }
}
