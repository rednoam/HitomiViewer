using ExtensionMethods;
using HitomiViewer.Encryption;
using HitomiViewer.Scripts;
using HitomiViewer.Scripts.Loaders;
using HitomiViewer.Style;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace HitomiViewer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        enum FolderSorts
        {
            Name,
            Creation,
            LastWrite,
            Size,
            Pages,
            SizePerPage
        }

        public static readonly string rootDir = AppDomain.CurrentDomain.BaseDirectory;
        public string path = string.Empty;
        public uint Page_itemCount = 25;
        public int Page = 1;
        public Func<string[], string[]> FolderSort;
        public List<Reader> Readers = new List<Reader>();
        public MainWindow()
        {
            new LoginClass().Test();
            InitializeComponent();
            Init();
            InitEvents();
        }

        private void Init()
        {
            CheckUpdate.Auto();
            this.MinWidth = 300;
            Global.MainWindow = this;
            string[] args = Environment.GetCommandLineArgs();
            bool relative = false;
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "/p" && args.Length - 1 > i)
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
            if (Global.FileEn)
                new TaskFactory().StartNew(() => LoadHitomi(path));
            else
                new TaskFactory().StartNew(() => LoadHitomi(path));
        }

        public void LoadHitomi(string path)
        {
            string[] @NotSorted = Directory.GetDirectories(path);
            LoadHitomi(NotSorted);
        }
        public void LoadHitomi(string[] files)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => label.Visibility = Visibility.Hidden));
            if (files.Length <= 0)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => label.Visibility = Visibility.Hidden));
                return;
            }
            string[] Folders = FolderSort(files);
            int i = 0;
            int SelectedPage = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.Background = new SolidColorBrush(Global.background);
                MainPanel.Children.Clear();
                if (SearchMode2.SelectedIndex == 1)
                    Folders = Folders.Reverse().ToArray();
                SelectedPage = Page_Index.SelectedIndex + 1;
                this.Title = string.Format("MainWindow - {0}페이지", SelectedPage);
            }));
            foreach (string folder in Folders.Where(x => Array.IndexOf(Folders, x) + 1 <= Page_itemCount * SelectedPage && Array.IndexOf(Folders, x) + 1 > (SelectedPage - 1) * Page_itemCount))
            {
                i++;
                Console.WriteLine("{0}: {1}", i, folder);
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".lock" };
                string[] innerFiles = Directory.GetFiles(folder).Where(file => allowedExtensions.Any(file.ToLower().EndsWith)).ToArray().ESort();
                if (innerFiles.Length <= 0) continue;
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    Hitomi h = new Hitomi
                    {
                        name = folder.Split(Path.DirectorySeparatorChar).Last(),
                        dir = folder,
                        page = innerFiles.Length,
                        files = innerFiles,
                        thumb = ImageProcessor.ProcessEncrypt(innerFiles.First()),
                        type = Hitomi.Type.Folder,
                        FolderByte = GetFolderByte(folder),
                        SizePerPage = GetSizePerPage(folder)
                    };
                    if (h.thumb == null) return;
                    label.FontSize = 100;
                    label.Content = i + "/" + Page_itemCount;
                    MainPanel.Children.Add(new HitomiPanel(h, this));
                    Console.WriteLine("Completed: {0}", innerFiles.First());
                }));
            }
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => label.Visibility = Visibility.Hidden));
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

        public int GetPage()
        {
            return (int)new CountBox("페이지", "원하는 페이지 수", 1).ShowDialog();
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
            foreach (HitomiPanel hitomiPanel in MainPanel.Children)
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
                            if (xlen > ylen) return 1;
                            if (xlen < ylen) return -1;
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
            label.Visibility = Visibility.Visible;
            label.FontSize = 100;
            InternetP parser = new InternetP(url: "https://api.hiyobi.me/list/" + GetPage());
            HiyobiLoader hiyobi = new HiyobiLoader();
            hiyobi.start = (int count) => label.Content = "0/" + count;
            hiyobi.update = (Hitomi h, int index, int max) =>
            {
                label.Content = $"{index}/{max}";
                MainPanel.Children.Add(new HitomiPanel(h, this));
            };
            hiyobi.end = () => label.Visibility = Visibility.Collapsed;
            parser.LoadJObject(hiyobi.Parser);
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
            InternetP parser = new InternetP(keyword: Hitomi_Search_Text.Text.Split(' ').ToList(), index: 1);
            HiyobiLoader hiyobi = new HiyobiLoader();
            hiyobi.start = (int count) => label.Content = "0/" + count;
            hiyobi.update = (Hitomi h, int index, int max) =>
            {
                label.Content = $"{index}/{max}";
                MainPanel.Children.Add(new HitomiPanel(h, this));
            };
            hiyobi.end = () => label.Visibility = Visibility.Collapsed;
            parser.HiyobiSearch(data => new InternetP(data: data).ParseJObject(hiyobi.Parser));
        }
        private void MenuHitomi_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            label.Visibility = Visibility.Visible;
            label.FontSize = 100;
            HitomiLoader hitomi = new HitomiLoader();
            hitomi.index = GetPage();
            hitomi.count = (int)Page_itemCount;
            hitomi.start = (int count) => label.Content = "0/" + count;
            hitomi.update = (Hitomi h, int index, int max) =>
            {
                label.Content = $"{index}/{max}";
                MainPanel.Children.Add(new HitomiPanel(h, this));
            };
            hitomi.end = () => label.Visibility = Visibility.Collapsed;
            hitomi.Parser();
        }
        private void Hitomi_Search_Text_KeyDown(object sender, KeyEventArgs e)
        {

        }
        private void Hitomi_Search_Button_Click(object sender, RoutedEventArgs e)
        {

        }
        private void OpenSetting_Click(object sender, RoutedEventArgs e)
        {
            new Settings().Show();
        }
        private async void FavoriteBtn_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            label.Visibility = Visibility.Visible;
            label.FontSize = 100;
            Config cfg = new Config();
            cfg.Load();
            List<string> favs = cfg.ArrayValue<string>("fav").ToList();
            favs = favs.Where(x => Directory.Exists(x) || x.isUrl()).Distinct().ToList();
            InternetP parser = new InternetP();
            parser.start = (int count) => label.Content = "0/" + count;
            parser.update = (Hitomi h, int index, int max) =>
            {
                label.Content = $"{index}/{max}";
                MainPanel.Children.Add(new HitomiPanel(h, this));
            };
            parser.end = () => label.Visibility = Visibility.Collapsed;
            await parser.LoadCompre(favs);
            label.Visibility = Visibility.Collapsed;
        }
        private void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            foreach (string item in Directory.GetDirectories(path))
            {
                string[] files = Directory.GetFiles(item);
                foreach (string file in files)
                {
                    if (Path.GetFileName(file) == "info.json") continue;
                    if (Path.GetFileName(file) == "info.txt") continue;
                    if (Path.GetExtension(file) == ".lock") continue;
                    byte[] org = File.ReadAllBytes(file);
                    byte[] enc = AES128.Encrypt(org, Global.Password);
                    File.Delete(file);
                    File.WriteAllBytes(file + ".lock", enc);
                }
            }
            MessageBox.Show("전체 암호화 완료");
        }
        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            foreach (string item in Directory.GetDirectories(path))
            {
                string[] files = Directory.GetFiles(item);
                foreach (string file in files)
                {
                    try
                    {
                        byte[] org = File.ReadAllBytes(file);
                        byte[] enc = AES128.Decrypt(org, Global.Password);
                        File.Delete(file);
                        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)), enc);
                    }
                    catch { }
                }
            }
            MessageBox.Show("전체 복호화 완료");
        }
    }
}
