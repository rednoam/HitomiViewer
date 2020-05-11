using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HitomiViewer;
using System.Windows.Interop;
using System.Windows;
using Bitmap = System.Drawing.Bitmap;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        [System.Runtime.InteropServices.DllImport("Shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);

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

        public static IEnumerable<string> FileInfoSort(this IEnumerable<string> list)
        {
            return list.Select(fn => new FileInfo(fn)).OrderBy(f => f.Name).Select(f => f.FullName);
        }

        public static void HitomiPanelSort(this StackPanel MainPanel)
        {
            HitomiPanel[] childs = MainPanel.Children.Cast<HitomiPanel>().ToArray();
            MainPanel.Children.Clear();
            Dictionary<string, HitomiPanel> panelKey = new Dictionary<string, HitomiPanel>();
            foreach (HitomiPanel child in childs)
            {
                string name = (((child.panel as DockPanel).Children[1] as DockPanel).Children[0] as Label).Content as string;
                panelKey.Add(name, child);
            }
            string[] names = panelKey.Select(k => Path.Combine(Global.MainWindow.path, k.Key)).IEESort();
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i].Split(Path.DirectorySeparatorChar).Last();
                Console.WriteLine(name);
                Global.MainWindow.label.Content = i + "/" + names.Length;
                MainPanel.Children.Add(panelKey[name]);
                panelKey[name].thumbNail.Source = panelKey[name].thumbNail.Source;
            }
        }

        public static Hitomi[] HitomiSort(this Hitomi[] hlist)
        {
            Dictionary<string, Hitomi> hitomiKey = new Dictionary<string, Hitomi>();
            foreach (Hitomi h in hlist)
            {
                string name = h.dir;
                hitomiKey.Add(name, h);
            }
            string[] names = hitomiKey.Select(h => h.Key).IEESort();
            List<Hitomi> hitomis = new List<Hitomi>();
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                hitomis.Add(hitomiKey[name]);
            }
            return hitomis.ToArray();
        }

        public static string[] ESort(this string[] list)
        {
            return list.Select(f => new FileInfo(f)).ToArray().ExplorerSort().Select(f => f.FullName).ToArray();
        }

        public static string[] IEESort(this IEnumerable<string> list)
        {
            return list.Select(f => new FileInfo(f)).ToArray().ExplorerSort().Select(f => f.FullName).ToArray();
        }

        public static FileInfo[] ExplorerSort(this FileInfo[] list)
        {
            Array.Sort(list, delegate (FileInfo x, FileInfo y) { return StrCmpLogicalW(x.Name, y.Name); });
            return list;
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

        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                                      IntPtr.Zero,
                                      Int32Rect.Empty,
                                      BitmapSizeOptions.FromEmptyOptions());
        }

        public static Hitomi Copy(this Hitomi hitomi)
        {
            return Hitomi.Copy(hitomi);
        }

        public static string RemoveSpace(this string s) => s.Replace(" ", string.Empty);

        static internal ImageSource doGetImageSourceFromResource(string psAssemblyName, string psResourceName)
        {
            Uri oUri = new Uri("pack://application:,,,/" + psAssemblyName + ";component/" + psResourceName, UriKind.RelativeOrAbsolute);
            return BitmapFrame.Create(oUri);
        }
    }
}
