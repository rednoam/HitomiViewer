using ExtensionMethods;
using HitomiViewer.Scripts;
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

namespace HitomiViewer.Processor
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
                        h.thumb = await ImageProcessor.LoadWebImageAsync("https:" + h.thumbpath);
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
                        thumb = await ImageProcessor.ProcessEncryptAsync(innerFiles.First()),
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
