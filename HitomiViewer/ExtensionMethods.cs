using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using HitomiViewer;

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static IEnumerable<string> CustomSort(this IEnumerable<string> list)
        {
            int maxLen = list.Select(s => s.Length).Max();

            return list.Select(s => new
            {
                OrgStr = s,
                SortStr = System.Text.RegularExpressions.Regex.Replace(s, @"(\d+)|(\D+)", m => m.Value.PadLeft(maxLen, char.IsDigit(m.Value[0]) ? ' ' : '\xffff'))
            })
            .OrderBy(x => x.SortStr)
            .Select(x => x.OrgStr);
        }

        public static BitmapImage ToImage(this byte[] array)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(array))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            // Bitmap 담을 메모리스트림 준비
            MemoryStream ms = new MemoryStream();   // 초기화
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);   // 

            // BitmapImage 로 변환
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            return bi;
        }

        public static Hitomi Copy(this Hitomi hitomi)
        {
            return Hitomi.Copy(hitomi);
        }
    }
}
