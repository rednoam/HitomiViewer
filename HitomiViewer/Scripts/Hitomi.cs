using ExtensionMethods;
using HitomiViewer.Style;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using static HitomiViewer.HitomiPanel;

namespace HitomiViewer
{
    public class Hitomi
    {
        public List<HitomiInfo.Tag> tags = new List<HitomiInfo.Tag>();
        public string name;
        public string dir;
        public int page;
        public string[] files;
        public string thumbpath;
        public double FolderByte;
        public double SizePerPage;
        public BitmapImage thumb;
        public BitmapImage[] images;

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
    }
}
