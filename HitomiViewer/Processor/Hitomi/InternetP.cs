using ExtensionMethods;
using HitomiViewer.Structs;
using HtmlAgilityPack;
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
        public async Task<Hitomi> HitomiData()
        {
            url = url ?? $"https://ltn.hitomi.la/galleryblock/{index}.html";
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
            for (var i = 0; i < table.ChildNodes.Count - 1; i += 2)
            {
                HtmlNode tr = table.ChildNodes[i + 1];
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
        public async Task<byte[]> LoadNozomi(string url = null)
        {
            url = url ?? this.url ?? "https://ltn.hitomi.la/index-all.nozomi";
            if (url.Last() == '/') url = url.Remove(url.Length - 1);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(index * 4, (index + count) * 4 - 1);
            var response = await client.GetAsync(url);
            var pageContents = await response.Content.ReadAsByteArrayAsync();
            return pageContents;
        }
    }
}
