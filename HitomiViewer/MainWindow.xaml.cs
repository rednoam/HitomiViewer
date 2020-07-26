using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;
using Bitmap = System.Drawing.Bitmap;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Threading;
using HitomiViewer.Scripts;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace HitomiViewer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        enum FolderSorts{
            Name,
            Creation,
            LastWrite,
            Size,
            Pages,
            SizePerPage
        }

        public readonly string rootDir = AppDomain.CurrentDomain.BaseDirectory;
        public string path = string.Empty;
        public uint Page_itemCount = 25;
        public Func<string[], string[]> FolderSort;
        public List<Reader> Readers = new List<Reader>();
        public MainWindow()
        {
            InitializeComponent();
            Init();
            InitEvents();
        }

        private void Init()
        {
            this.MinWidth = 300;
            Global.MainWindow = this;
            string[] args = Environment.GetCommandLineArgs();
            bool relative = false;
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "/p" && args.Length-1 > i)
                {
                    if (relative) path = Path.Combine(rootDir, args[i + 1]);
                    else path = args[i + 1];
                }
                if (arg == "/r") relative = true;
            }
            if (path == string.Empty) path = Path.Combine(rootDir, "hitomi_downloaded");
            else
            {
                if (relative) path = Path.Combine(rootDir, path);
            }
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Invaild Path");
                path = Path.Combine(rootDir, "hitomi_downloaded");
            }
            SearchMode1.SelectedIndex = 0;
            SetFolderSort(FolderSorts.Name);
        }

        private void InitEvents()
        {
            this.Loaded += MainWindow_Loaded;
        }

        private void DelayRegistEvents()
        {
            SearchMode1.SelectionChanged += SearchMode1_SelectionChanged;
            SearchMode2.SelectionChanged += SearchMode2_SelectionChanged;
            Page_Index.SelectionChanged += Page_Index_SelectionChanged;
            Page_ItemCount.SelectionChanged += Page_ItemCount_SelectionChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            label.FlowDirection = FlowDirection.RightToLeft;
            label.FontSize = 100;
            label.Content = "로딩중";
            label.Margin = new Thickness(352 - label.Content.ToString().Length * 11, 240, 0, 0);
            label.Visibility = Visibility.Visible;
            this.Background = new SolidColorBrush(Global.background);
            MainPanel.Children.Clear();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
                //path = new InputBox("불러올 하위 폴더이름", "폴더 지정", "폴더 이름").ShowDialog();
            int pages = Directory.GetDirectories(path).Length / 25 + 1;
            for (int i = 0; i < pages; i++)
            {
                Page_Index.Items.Add(i + 1);
            }
            Page_Index.SelectedIndex = 0;
            Page_ItemCount.SelectedIndex = 3;
            SearchMode2.SelectedIndex = 0;
            DelayRegistEvents();
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }

        public void LoadHitomi(string path)
        {
            string[] @NotSorted = Directory.GetDirectories(path);
            LoadHitomi(NotSorted);
        }

        public void LoadHitomi(string[] files)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate { label.Visibility = Visibility.Visible; }));
            if (files.Length <= 0)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate { label.Visibility = Visibility.Hidden; }));
                return;
            }
            string[] Folders = FolderSort(files);
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                this.Background = new SolidColorBrush(Global.background);
                MainPanel.Children.Clear();
                if (SearchMode2.SelectedIndex == 1)
                    Folders = Folders.Reverse().ToArray();
            }));
            int i = 0;
            int SelectedPage = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                SelectedPage = Page_Index.SelectedIndex + 1;
                this.Title = string.Format("MainWindow - {0}페이지", SelectedPage);
            }));
            foreach (string folder in Folders.Where(x => Array.IndexOf(Folders, x) + 1 <= Page_itemCount * SelectedPage && Array.IndexOf(Folders, x) + 1 > (SelectedPage - 1) * Page_itemCount))
            {
                i++;
                Console.WriteLine("{0}: {1}", i, folder);
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                string[] @NotSorted = Directory.GetFiles(folder).Where(file => allowedExtensions.Any(file.ToLower().EndsWith)).ToArray().ESort().ToArray();
                if (NotSorted.Length <= 0) continue;
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    label.FontSize = 100;
                    label.Content = i + "/" + Page_itemCount;
                    string[] innerFiles = NotSorted.ESort().ToArray();
                    Hitomi h = new Hitomi
                    {
                        name = folder.Split(Path.DirectorySeparatorChar).Last(),
                        dir = folder,
                        page = innerFiles.Length,
                        thumb = new BitmapImage(new Uri(innerFiles.First())),
                        type = Hitomi.Type.Folder
                    };
                    h.FolderByte = GetFolderByte(folder);
                    h.SizePerPage = GetSizePerPage(folder);
                    MainPanel.Children.Add(new HitomiPanel(h, this));
                    h.thumb = null;
                    Console.WriteLine("Completed: {0}", innerFiles.First());
                }));
            }
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                label.Visibility = Visibility.Hidden;
            }));
            GC.Collect();
        }

        public void RemoveChild(HitomiPanel panel, string dir)
        {
            MainPanel.Children.Clear();
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine("GC Check");
                        Directory.Delete(dir, true);
                        Console.WriteLine("Removed");
                        LoadHitomi(path);
                        break;
                    }
                    catch
                    {
                        Console.WriteLine("GC Fail");
                        Console.WriteLine("GC Run");
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        Thread.Sleep(2000);
                    }
                }
            });
            thread.Start();
        }

        public double GetFolderByte(string dir)
        {
            DirectoryInfo info = new DirectoryInfo(dir);
            double FolderByte = info.EnumerateFiles().Sum(f => f.Length);
            return FolderByte;
        }

        public double GetSizePerPage(string dir)
        {
            DirectoryInfo info = new DirectoryInfo(dir);
            double FolderByte = info.EnumerateFiles().Sum(f => f.Length);
            double SizePerPage = FolderByte / info.GetFiles().Length;
            return SizePerPage;
        }

        public long GetWebSize(string url)
        {
            System.Net.WebClient client = new System.Net.WebClient();
            client.OpenRead(url);
            long bytes_total = Convert.ToInt64(client.ResponseHeaders["Content-Length"]);
            return bytes_total;
        }

        public BitmapImage LoadImage(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return null;
                System.Net.WebClient wc = new System.Net.WebClient();
                Byte[] MyData = wc.DownloadData(url);
                wc.Dispose();
                BitmapImage bimgTemp = new BitmapImage();
                bimgTemp.BeginInit();
                bimgTemp.StreamSource = new MemoryStream(MyData);
                bimgTemp.EndInit();
                return bimgTemp;
            }
            catch
            {
                return null;
            }
        }

        private void SetColor()
        {
            foreach(HitomiPanel hitomiPanel in MainPanel.Children)
            {
                HitomiPanel.ChangeColor(hitomiPanel);
            }
        }
        private void SetFolderSort(FolderSorts sorts)
        {
            switch (sorts)
            {
                case FolderSorts.Name:
                    FolderSort = (string[] arr) =>
                    {
                        return arr.ESort().ToArray();
                    };
                    break;
                case FolderSorts.Creation:
                    FolderSort = (string[] arr) =>
                    {
                        var arr2 = arr.Select(f => new FileInfo(f)).ToArray();
                        Array.Sort(arr2, delegate (FileInfo x, FileInfo y) { return DateTime.Compare(x.CreationTime, y.CreationTime); });
                        return arr2.Select(f => f.FullName).ToArray();
                    };
                    break;
                case FolderSorts.LastWrite:
                    FolderSort = (string[] arr) =>
                    {
                        var arr2 = arr.Select(f => new FileInfo(f)).ToArray();
                        Array.Sort(arr2, delegate (FileInfo x, FileInfo y) { return DateTime.Compare(x.LastWriteTime, y.LastWriteTime); });
                        return arr2.Select(f => f.FullName).ToArray();
                    };
                    break;
                case FolderSorts.Size:
                    FolderSort = (string[] arr) =>
                    {
                        var arr2 = arr.Select(f => new DirectoryInfo(f)).ToArray();
                        Array.Sort(arr2, delegate (DirectoryInfo x, DirectoryInfo y)
                        {
                            long xlen = x.EnumerateFiles().Sum(f => f.Length);
                            long ylen = y.EnumerateFiles().Sum(f => f.Length);
                            if (xlen == ylen) return 0;
                            if (xlen >  ylen) return 1;
                            if (xlen <  ylen) return -1;
                            return 0;
                        });
                        return arr2.Select(f => f.FullName).ToArray();
                    };
                    break;
                case FolderSorts.Pages:
                    FolderSort = (string[] arr) =>
                    {
                        var arr2 = arr.ToArray();
                        Array.Sort(arr2, delegate (string x, string y)
                        {
                            long xlen = Directory.GetFiles(x).Length;
                            long ylen = Directory.GetFiles(y).Length;
                            if (xlen == ylen) return 0;
                            if (xlen > ylen) return 1;
                            if (xlen < ylen) return -1;
                            return 0;
                        });
                        return arr2.ToArray();
                    };
                    break;
                case FolderSorts.SizePerPage:
                    FolderSort = (string[] arr) =>
                    {
                        var arr2 = arr.Select(f => new DirectoryInfo(f)).ToArray();
                        Array.Sort(arr2, delegate (DirectoryInfo x, DirectoryInfo y)
                        {
                            long xlen = x.EnumerateFiles().Sum(f => f.Length) / x.GetFiles().Length;
                            long ylen = y.EnumerateFiles().Sum(f => f.Length) / y.GetFiles().Length;
                            if (xlen == ylen) return 0;
                            if (xlen > ylen) return 1;
                            if (xlen < ylen) return -1;
                            return 0;
                        });
                        return arr2.Select(f => f.FullName).ToArray();
                    };
                    break;
            }
        }
        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            Global.background = Colors.Black;
            Global.imagecolor = Color.FromRgb(116, 116, 116);
            Global.Menuground = Color.FromRgb(33, 33, 33);
            Global.MenuItmclr = Color.FromRgb(76, 76, 76);
            Global.panelcolor = Color.FromRgb(76, 76, 76);
            Global.fontscolor = Colors.White;
            Global.outlineclr = Colors.White;
            this.Background = new SolidColorBrush(Global.background);
            MainMenuBackground.Color = Global.Menuground;
            foreach (MenuItem menuItem in MainMenu.Items)
            {
                menuItem.Background = new SolidColorBrush(Global.MenuItmclr);
                menuItem.Foreground = new SolidColorBrush(Global.fontscolor);
                foreach (MenuItem item in menuItem.Items)
                    item.Foreground = new SolidColorBrush(Colors.Black);
            }
            foreach (Reader reader in Readers)
                reader.ChangeMode();
            SetColor();
            //LoadHitomi(Path.Combine(rootDir, folder));
        }
        private void DarkMode_Unchecked(object sender, RoutedEventArgs e)
        {
            Global.background = Colors.White;
            Global.imagecolor = Colors.LightGray;
            Global.Menuground = Color.FromRgb(240, 240, 240);
            Global.MenuItmclr = Colors.White;
            Global.panelcolor = Colors.White;
            Global.fontscolor = Colors.Black;
            Global.outlineclr = Colors.Black;
            this.Background = new SolidColorBrush(Global.background);
            MainMenuBackground.Color = Global.Menuground;
            foreach (MenuItem menuItem in MainMenu.Items)
            {
                menuItem.Background = new SolidColorBrush(Global.MenuItmclr);
                menuItem.Foreground = new SolidColorBrush(Global.fontscolor);
                foreach (MenuItem item in menuItem.Items)
                    item.Foreground = new SolidColorBrush(Colors.Black);
            }
            foreach (Reader reader in Readers)
                reader.ChangeMode();
            SetColor();
            //LoadHitomi(Path.Combine(rootDir, folder));
        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                //Normal
                if (WindowStyle == WindowStyle.None && WindowState == WindowState.Maximized)
                {
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;
                }
                else if (WindowStyle == WindowStyle.SingleBorderWindow && WindowState == WindowState.Normal)
                {
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                }
                //Maximized
                else if (WindowStyle == WindowStyle.SingleBorderWindow && WindowState == WindowState.Maximized)
                {
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Normal;
                    this.WindowState = WindowState.Maximized;
                }
            }
            else if (e.Key == Key.Escape)
            {
                if (WindowStyle == WindowStyle.None && WindowState == WindowState.Maximized)
                {
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;
                }
            }
            else if (e.Key == Key.R)
            {
                label.FontSize = 100;
                label.Content = "로딩중";
                label.Visibility = Visibility.Visible;
                this.Background = new SolidColorBrush(Global.background);
                MainPanel.Children.Clear();
                new TaskFactory().StartNew(() => LoadHitomi(path));
            }
        }
        private void SearchMode1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FolderSorts SortTypes = FolderSorts.Name;
            switch (SearchMode1.SelectedIndex)
            {
                case 0:
                    SortTypes = FolderSorts.Name;
                    break;
                case 1:
                    SortTypes = FolderSorts.Creation;
                    break;
                case 2:
                    SortTypes = FolderSorts.LastWrite;
                    break;
                case 3:
                    SortTypes = FolderSorts.Size;
                    break;
                case 4:
                    SortTypes = FolderSorts.Pages;
                    break;
                case 5:
                    SortTypes = FolderSorts.SizePerPage;
                    break;
            }
            SetFolderSort(SortTypes);
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }
        private void SearchMode2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }
        private void Page_Index_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }
        private void Page_ItemCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Page_itemCount = uint.Parse(((ComboBoxItem)Page_ItemCount.SelectedItem).Content.ToString());
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }
        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }
        private void Search_Button_Click(object sender, RoutedEventArgs e)
        {
            string SearchText = Search_Text.Text;
            string[] files = Directory.GetDirectories(path).Where(x => x.RemoveSpace().Contains(SearchText.RemoveSpace())).ToArray();
            new TaskFactory().StartNew(() => LoadHitomi(files));
        }
        private void Search_Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string SearchText = Search_Text.Text;
                string[] files = Directory.GetDirectories(path).Where(x => x.RemoveSpace().Contains(SearchText.RemoveSpace())).ToArray();
                new TaskFactory().StartNew(() => LoadHitomi(files));
            }
        }
        private void MenuHiyobi_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            InternetP parser = new InternetP();
            int index = (int)new CountBox("페이지", "원하는 페이지 수", 1).ShowDialog();
            parser.url = "https://api.hiyobi.me/list/" + index;
            parser.LoadJObject(async (JObject jObject) =>
            {
                foreach (JToken tk in jObject["list"])
                {
                    parser.url = $"https://cdn.hiyobi.me/data/json/{tk["id"]}_list.json";
                    JArray imgs = await parser.TryLoadJArray();
                    if (imgs == null) continue;
                    Hitomi h = new Hitomi
                    {
                        id = tk["id"].ToString(),
                        name = tk["title"].ToString(),
                        dir = $"https://hiyobi.me/reader/{tk["id"]}",
                        page = imgs.Count,
                        thumb = LoadImage($"https://cdn.hiyobi.me/tn/{tk["id"]}.jpg"),
                        type = Hitomi.Type.Hiyobi
                    };
                    Int64 size = 0;
                    h.files = imgs.ToList().Select(x => $"https://cdn.hiyobi.me/data/{tk["id"]}/{x["name"]}").ToArray();
                    h.FolderByte = size;
                    h.SizePerPage = size / imgs.Count;
                    foreach(JToken tags in tk["tags"])
                    {
                        HitomiPanel.HitomiInfo.Tag tag = new HitomiPanel.HitomiInfo.Tag();
                        if (tags["value"].ToString().Contains(":"))
                        {
                            tag.types = (HitomiViewer.Tag.Types)Enum.Parse(typeof(HitomiViewer.Tag.Types), tags["value"].ToString().Split(':')[0]);
                            tag.name = tags["display"].ToString();
                        }
                        else
                        {
                            tag.types = HitomiViewer.Tag.Types.tag;
                            tag.name = tags["display"].ToString();
                        }
                        h.tags.Add(tag);
                    }
                    MainPanel.Children.Add(new HitomiPanel(h, this));
                    Console.WriteLine($"Completed: https://cdn.hiyobi.me/tn/{tk["id"]}.jpg");
                }
            });
        }
        private void Hiyobi_Search_Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Hiyobi_Search_Button_Click(sender, null);
        }
        private void Hiyobi_Search_Button_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            label.Visibility = Visibility.Visible;
            label.FontSize = 100;
            InternetP parser = new InternetP();
            parser.keyword = Hiyobi_Search_Text.Text.Split(' ').ToList();
            parser.index = 1;
            parser.HiyobiSearch(data => new InternetP(data: data).ParseJObject(async jobject =>
            {
                label.Content = 0 + "/" + jobject["list"].ToList().Count;
                foreach (JToken tk in jobject["list"])
                {
                    label.Content = jobject["list"].ToList().IndexOf(tk) + "/" + jobject["list"].ToList().Count;
                    parser = new InternetP();
                    parser.url = $"https://cdn.hiyobi.me/data/json/{tk["id"]}_list.json";
                    JArray imgs = await parser.TryLoadJArray();
                    if (imgs == null) continue;
                    Hitomi h = new Hitomi
                    {
                        id = tk["id"].ToString(),
                        name = tk["title"].ToString(),
                        dir = $"https://hiyobi.me/reader/{tk["id"]}",
                        page = imgs.Count,
                        thumb = LoadImage($"https://cdn.hiyobi.me/tn/{tk["id"]}.jpg"),
                        type = Hitomi.Type.Hiyobi
                    };
                    Int64 size = 0;
                    h.files = imgs.ToList().Select(x => $"https://cdn.hiyobi.me/data/{tk["id"]}/{x["name"]}").ToArray();
                    h.FolderByte = size;
                    h.SizePerPage = size / imgs.Count;
                    foreach (JToken tags in tk["tags"])
                    {
                        HitomiPanel.HitomiInfo.Tag tag = new HitomiPanel.HitomiInfo.Tag();
                        if (tags["value"].ToString().Contains(":"))
                        {
                            tag.types = (HitomiViewer.Tag.Types)Enum.Parse(typeof(HitomiViewer.Tag.Types), tags["value"].ToString().Split(':')[0]);
                            tag.name = tags["display"].ToString();
                        }
                        else
                        {
                            tag.types = HitomiViewer.Tag.Types.tag;
                            tag.name = tags["display"].ToString();
                        }
                        h.tags.Add(tag);
                    }
                    MainPanel.Children.Add(new HitomiPanel(h, this));
                    Console.WriteLine($"Completed: https://cdn.hiyobi.me/tn/{tk["id"]}.jpg");
                }
            }));
        }
        private async void MenuHitomi_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            InternetP parser = new InternetP();
            int index = (int)new CountBox("페이지", "원하는 페이지 수", 1).ShowDialog();
            parser.index = (index - 1) * unchecked((int)this.Page_itemCount);
            parser.count = unchecked((int)this.Page_itemCount);
            parser.url = "https://ltn.hitomi.la/index-all.nozomi";
            int[] ids = parser.ByteArrayToIntArray(await parser.LoadNozomi());
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
                h.thumb = LoadImage("https:"+h.thumbpath);
                MainPanel.Children.Add(new HitomiPanel(h, this));
            }
        }
        private void Hitomi_Search_Text_KeyDown(object sender, KeyEventArgs e)
        {

        }
        private void Hitomi_Search_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
