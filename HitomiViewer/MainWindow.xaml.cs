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

namespace HitomiViewer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly string rootDir = AppDomain.CurrentDomain.BaseDirectory;
        public string path = string.Empty;
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
        }

        private void InitEvents()
        {
            this.Loaded += MainWindow_Loaded;
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
                path = Path.Combine(rootDir, new InputBox("불러올 하위 폴더이름", "폴더 지정", "폴더 이름").ShowDialog());
            SearchMode.SelectedIndex = 0;
            SearchMode.SelectionChanged += SearchMode_SelectionChanged;
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }

        public void LoadHitomi(string path)
        {
            string[] @NotSorted = Directory.GetDirectories(path);
            if (NotSorted.Length <= 0)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate { label.Visibility = Visibility.Hidden; }));
                return;
            }
            string[] Folders = NotSorted.ESort().ToArray();
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                this.Background = new SolidColorBrush(Global.background);
                MainPanel.Children.Clear();
                if (SearchMode.SelectedIndex == 1)
                    Folders = Folders.Reverse().ToArray();
            }));
            int i = 0;
            foreach (string folder in Folders)
            {
                i++;
                @NotSorted = Directory.GetFiles(folder, "*.jpg");
                if (NotSorted.Length <= 0) continue;
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    label.FontSize = 100;
                    label.Content = i + "/" + Folders.Length;
                    string[] innerFiles = NotSorted.ESort().ToArray();
                    Hitomi h = new Hitomi
                    {
                        name = folder.Split(Path.DirectorySeparatorChar).Last(),
                        dir = folder,
                        page = innerFiles.Length,
                        thumb = new BitmapImage(new Uri(innerFiles.First()))
                    };
                    MainPanel.Children.Add(new HitomiPanel(h));
                }));
            }
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                label.Visibility = Visibility.Hidden;
            }));
        }

        private void SetColor()
        {
            foreach(HitomiPanel hitomiPanel in MainPanel.Children)
            {
                HitomiPanel.ChangeColor(hitomiPanel);
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
        private void SearchMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }
    }
}
