using ExtensionMethods;
using HitomiViewer.Scripts.Loaders;
using HitomiViewer.Structs;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Scripts
{
    partial class InternetP
    {
        public string url { get; set; }
        public string data { get; set; }
        public List<string> keyword { get; set; }
        public int index { get; set; }
        public int count { get; set; }
        public Action<Hitomi, int, int> update = null;
        public Action<int> start = null;
        public Action end = null;

        public InternetP(string url = null, string data = null, List<string> keyword = null, int? index = null)
        {
            if (url != null) this.url = url;
            if (data != null) this.data = data;
            if (keyword != null) this.keyword = keyword;
            if (index != null) this.index = index.Value;
        }

        public InternetP SetData(string data)
        {
            this.data = data;
            return this;
        }

        public async void LoadJObject(Action<JObject> callback) => callback(await LoadJObject());
        public async Task<JObject> LoadJObject()
        {
            string html = await Load(url);
            return JObject.Parse(html);
        }
        public async void LoadJArray(Action<JArray> callback) => callback(await LoadJArray());
        public async Task<JArray> LoadJArray()
        {
            string html = await Load(url);
            return JArray.Parse(html);
        }

        public async void TryLoadJObject(Action<JObject> callback) => callback(await TryLoadJObject());
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
        public async void TryLoadJArray(Action<JArray> callback) => callback(await TryLoadJArray());
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

        public void ParseJObject(Action<JObject> callback) => callback(ParseJObject());
        public JObject ParseJObject()
        {
            return JObject.Parse(data);
        }
        public void ParseJArray(Action<JArray> callback) => callback(ParseJArray());
        public JArray ParseJArray()
        {
            return JArray.Parse(data);
        }

        public async void HiyobiList(Action<List<Hitomi>> callback) => callback(await HiyobiList());
        public async void HiyobiFiles(Action<List<HitomiFile>> callback) => callback(await HiyobiFiles());
        public async void HiyobiSearch(Action<string> callback) => callback(await HiyobiSearch());

        #region Hiyobi
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
        #endregion
        #region Hitomi
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
            image = image ?? doc.DocumentNode.SelectSingleNode("//div[@class=\"cg-img1\"]/img");
            h.thumbpath = image.GetAttributeValue("src", "");
            if (h.thumbpath == "")
                h.thumbpath = image.GetDataAttribute("src").Value;
            //HtmlNode artist = doc.DocumentNode.SelectSingleNode("//div[@class=\"artist-list\"]/ul");
            HtmlNodeCollection artists = doc.DocumentNode.SelectNodes("//div[@class=\"artist-list\"]/ul/li");
            if (artists != null)
            {
                h.authors = artists.Select(x => x.InnerText).ToArray();
                h.author = string.Join(", ", h.authors);
            }
            else
            {
                h.authors = new string[0];
                h.author = "";
            }
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
        public async Task<Hitomi> HitomiGalleryData(Hitomi org)
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
            org.language = jObject.StringValue("language");
            org.id = jObject.StringValue("id");
            org.designType = DesignTypeFromString(jObject.StringValue("type"));
            return org;
        }
        public async Task<Hitomi> HitomiData2()
        {
            url = $"https://ltn.hitomi.la/galleryblock/{index}.html";
            Hitomi h = await HitomiData();
            url = $"https://ltn.hitomi.la/galleries/{index}.js";
            JObject info = await HitomiGalleryInfo();
            h.type = Hitomi.Type.Hitomi;
            h.tags = HitomiTags(info);
            h.files = HitomiFiles(info).ToArray();
            h.page = h.files.Length;
            h.thumb = ImageProcessor.LoadWebImage("https:" + h.thumbpath);
            h.Json = info;
            return await HitomiGalleryData(h);
        }
        public List<Tag> HitomiTags(JObject jObject)
        {
            List<Tag> tags = new List<Tag>();
            foreach (JToken tag1 in jObject["tags"])
            {
                Tag tag = new Tag();
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
        #endregion
        public HitomiInfo.Type DesignTypeFromInt(int s)
        {
            switch (s)
            {
                case 1:
                    return HitomiInfo.Type.doujinshi;
                case 2:
                    return HitomiInfo.Type.manga;
                case 3:
                    return HitomiInfo.Type.artistcg;
                default:
                    return HitomiInfo.Type.none;
            }
        }
        public HitomiInfo.Type DesignTypeFromString(string s)
        {
            switch (s)
            {
                case "doujinshi":
                    return HitomiInfo.Type.doujinshi;
                case "artistcg":
                    return HitomiInfo.Type.artistcg;
                case "manga":
                    return HitomiInfo.Type.manga;
                default:
                    return HitomiInfo.Type.none;
            }
        }
        public async Task<List<Hitomi>> LoadCompre(List<string> items)
        {
            List<Hitomi> res = new List<Hitomi>();
            start(items.Count);
            foreach (string item in items)
            {
                if (item.isUrl())
                {
                    Uri uri = new Uri(item);
                    if (uri.Host == "hiyobi.me")
                    {
                        string id = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
                        Hitomi h = await new HiyobiLoader(text: id).Parser();
                        res.Add(h);
                        update(h, items.IndexOf(item), items.Count);
                    }
                    if (uri.Host == "hitomi.la")
                    {
                        string id = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
                        this.url = $"https://ltn.hitomi.la/galleryblock/{id}.html";
                        this.index = int.Parse(id);
                        Hitomi h = await HitomiData();
                        this.url = $"https://ltn.hitomi.la/galleries/{id}.js";
                        JObject info = await HitomiGalleryInfo();
                        h.type = Hitomi.Type.Hitomi;
                        h.tags = HitomiTags(info);
                        h.files = HitomiFiles(info).ToArray();
                        h.page = h.files.Length;
                        h.thumb = ImageProcessor.LoadWebImage("https:" + h.thumbpath);
                        update(h, items.IndexOf(item), items.Count);
                    }
                }
                else
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".lock" };
                    string[] innerFiles = Directory.GetFiles(item).Where(file => allowedExtensions.Any(file.ToLower().EndsWith)).ToArray().ESort();
                    if (innerFiles.Length <= 0) continue;
                    Hitomi h = new Hitomi
                    {
                        name = item.Split(Path.DirectorySeparatorChar).Last(),
                        dir = item,
                        page = innerFiles.Length,
                        files = innerFiles,
                        thumb = ImageProcessor.ProcessEncrypt(innerFiles.First()),
                        type = Hitomi.Type.Folder,
                        FolderByte = File2.GetFolderByte(item),
                        SizePerPage = File2.GetSizePerPage(item)
                    };
                    update(h, items.IndexOf(item), items.Count);
                }
            }
            end();
            return res;
        }
        public async Task<bool> isHiyobi(int? index = null)
        {
            try
            {
                await Load($"https://cdn.hiyobi.me/data/json/{(index ?? this.index).ToString()}.json");
                return true;
            }
            catch { return false; }
        }

        public long GetWebSize(string url = null)
        {
            System.Net.WebClient client = new System.Net.WebClient();
            client.OpenRead(url ?? this.url);
            long bytes_total = Convert.ToInt64(client.ResponseHeaders["Content-Length"]);
            return bytes_total;
        }
        public async Task<string> Load(string url = null)
        {
            url = url ?? this.url;
            if (url.Last() == '/') url = url.Remove(url.Length - 1);
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
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
    }
}
