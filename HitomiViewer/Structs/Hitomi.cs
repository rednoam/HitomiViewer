using ExtensionMethods;
using HitomiViewer.Processor;
using HitomiViewer.Scripts;
using HitomiViewer.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace HitomiViewer
{
    public class Hitomi
    {
        public enum Type
        {
            None,
            Folder,
            Hiyobi,
            Hitomi
        }

        public List<Tag> tags = new List<Tag>();
        public string id;
        public string name;
        public string dir;
        public string author;
        public string thumbpath;
        public string language;
        public string[] files;
        public string[] authors;
        public int page;
        public bool encrypted;
        public double FolderByte;
        public double SizePerPage;
        public BitmapImage thumb;
        public BitmapImage[] images;
        public Type type = Type.None;
        public HitomiInfo.Type designType;
        public JToken Json;

        public void AutoAuthor() => author = string.Join(", ", authors);
        public void Save(string path) => File.WriteAllText(path, JObject.FromObject(this).ToString());

        public static Hitomi Copy(Hitomi hitomi)
        {
            Hitomi h = new Hitomi();
            h.name = hitomi.name;
            h.dir = hitomi.dir;
            h.page = hitomi.page;
            h.files = hitomi.files;
            h.thumbpath = hitomi.thumbpath;
            h.FolderByte = hitomi.FolderByte;
            h.SizePerPage = hitomi.SizePerPage;
            h.thumb = hitomi.thumb;
            h.images = hitomi.images;
            return h;
        }
        public static Hitomi GetHitomi(string path, string patturn = Global.basicPatturn)
        {
            string[] innerFiles = System.IO.Directory.GetFiles(path, patturn).ESort().ToArray();
            Hitomi h = new Hitomi
            {
                name = path.Split(System.IO.Path.DirectorySeparatorChar).Last(),
                dir = path,
                page = innerFiles.Length,
                thumb = new BitmapImage(new System.Uri(innerFiles.First()))
            };
            return h;
        }
        public static Hitomi FromJObject(JObject obj)
        {
            return new Hitomi
            {
                id = obj.StringValue("id"),
                name = obj.StringValue("name"),
                dir = obj.StringValue("dir"),
                author = obj.StringValue("author"),
                thumbpath = obj.StringValue("thumbpath"),
                language = obj.StringValue("language"),
                page = obj.IntValue("page") ?? 0,
                encrypted = obj.BoolValue("encrypted") ?? false,
                FolderByte = obj.DoubleValue("FolderByte") ?? 0,
                SizePerPage = obj.DoubleValue("SizePerPage") ?? 0,
                thumb = ImageProcessor.ProcessEncrypt(obj.StringValue("thumbpath")),
                images = null,
                type = (Type)obj.IntValue("type"),
                Json = obj["Json"]
            };
        }
        public static async Task<Hitomi> FromJObjectAsync(JObject obj)
        {
            return new Hitomi
            {
                id = obj.StringValue("id"),
                name = obj.StringValue("name"),
                dir = obj.StringValue("dir"),
                author = obj.StringValue("author"),
                thumbpath = obj.StringValue("thumbpath"),
                language = obj.StringValue("language"),
                page = obj.IntValue("page") ?? 0,
                encrypted = obj.BoolValue("encrypted") ?? false,
                FolderByte = obj.DoubleValue("FolderByte") ?? 0,
                SizePerPage = obj.DoubleValue("SizePerPage") ?? 0,
                thumb = await ImageProcessor.ProcessEncryptAsync(obj.StringValue("thumbpath")),
                images = null,
                type = (Type)obj.IntValue("type"),
                Json = obj["Json"]
            };
        }
    }
    class HitomiFile
    {
        public string path { get; set; }
        public string hash { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public bool hasavif { get; set; }
        public bool haswebp { get; set; }
    }
    public class HitomiInfo
    {
        public static HitomiInfo Parse(HitomiInfoOrg org)
        {
            HitomiInfo info = new HitomiInfo();
            info.Title = org.Title;
            info.Author = org.Author;
            info.Number = int.Parse(org.Number);
            {
                List<Tag> tags = new List<Tag>();
                string[] arr = org.Tags.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in arr)
                {
                    Tag tag = new Tag();
                    if (item.Contains(":"))
                    {
                        tag.types = (Tag.Types)Enum.Parse(typeof(Tag.Types), item.Split(':')[0]);
                        tag.name = string.Join(":", item.Split(':').Skip(1));
                    }
                    else
                    {
                        tag.types = Tag.Types.tag;
                        tag.name = item;
                    }

                    tags.Add(tag);
                }
                info.Tags = tags.ToArray();
            }
            return info;
        }
        public int Number { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Group { get; set; }
        public Type Types { get; set; }
        public string Series { get; set; }
        public string Character { get; set; }
        public Tag[] Tags { get; set; }
        public string Language { get; set; }

        public enum Type
        {
            doujinshi,
            artistcg,
            manga,
            gamecg,
            none,
        }
    }
    public class HitomiInfoOrg
    {
        public string Number { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Group { get; set; }
        public string Types { get; set; }
        public string Series { get; set; }
        public string Character { get; set; }
        public string Tags { get; set; }
        public string Language { get; set; }
    }
}
