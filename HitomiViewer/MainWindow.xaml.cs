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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace HitomiViewer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly string rootDir = AppDomain.CurrentDomain.BaseDirectory;
        public Color background = Colors.White;
        public Color Menuground = Color.FromRgb(240, 240, 240);
        public Color MenuItmclr = Colors.White;
        public Color childcolor = Colors.White;
        public Color imagecolor = Colors.LightGray;
        public Color panelcolor = Colors.White;
        public Color fontscolor = Colors.Black;
        public Color outlineclr = Colors.Black;
        public List<Reader> Readers = new List<Reader>();
        public string folder = "hitomi_downloaded";
        public MainWindow()
        {
            InitializeComponent();
            Init();
            InitEvents();
        }

        private void Init()
        {
            this.MinWidth = 300;
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
            this.Background = new SolidColorBrush(background);
            MainPanel.Children.Clear();
            if (!Directory.Exists(Path.Combine(rootDir, folder)))
                folder = new InputBox("불러올 하위 폴더이름", "폴더 지정", "폴더 이름").ShowDialog();
            SearchMode.SelectedIndex = 0;
            SearchMode.SelectionChanged += SearchMode_SelectionChanged;
            new TaskFactory().StartNew(() => LoadHitomi(Path.Combine(rootDir, folder)));
            //LoadHitomi(Path.Combine(rootDir, "hitomi_downloaded"));
        }

        public void LoadHitomi(string path)
        {
            string[] Folders = Directory.GetDirectories(path).CustomSort().ToArray();
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                this.Background = new SolidColorBrush(background);
                MainPanel.Children.Clear();
                if (SearchMode.SelectedIndex == 1)
                    Folders = Folders.Reverse().ToArray();
            }));
            int i = 0;
            foreach (string folder in Folders)
            {
                if (!(Directory.GetFiles(folder, "*.jpg").Length >= 1)) continue;
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    label.FontSize = 100;
                    label.Content = "로딩중" + ++i + "/" + Folders.Length;
                    label.Margin = new Thickness(352 - label.Content.ToString().Length * 11, 240, 0, 0);
                    string[] innerFiles = Directory.GetFiles(folder, "*.jpg").CustomSort().ToArray();
                    const int Magnif = 4;
                    Hitomi h = new Hitomi
                    {
                        name = folder.Split(Path.DirectorySeparatorChar).Last(),
                        dir = folder,
                        page = innerFiles.Length,
                        files = innerFiles,
                        thumb = new BitmapImage(new Uri(innerFiles.First()))
                    };
                    StackPanel panel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Height = 100,
                        Margin = new Thickness(5),
                        Background = new SolidColorBrush(panelcolor)
                    };
                    Border border = new Border
                    {
                        Background = new SolidColorBrush(imagecolor)
                    };
                    ImageBrush ib = new ImageBrush
                    {
                        ImageSource = h.thumb
                    };
                    FrameworkElementFactory elemImage = new FrameworkElementFactory(typeof(Image));
                    double b = panel.Height / h.thumb.Width;
                    elemImage.SetValue(Image.WidthProperty, b * h.thumb.Width * Magnif);
                    elemImage.SetValue(Image.HeightProperty, b * h.thumb.Height * Magnif);
                    elemImage.SetValue(Image.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                    elemImage.SetValue(Image.SourceProperty, h.thumb);
                    FrameworkElementFactory elemborder = new FrameworkElementFactory(typeof(Border));
                    elemborder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
                    elemborder.SetValue(Border.BorderBrushProperty, new SolidColorBrush(outlineclr));
                    elemborder.AppendChild(elemImage);
                    FrameworkElementFactory elemFactory = new FrameworkElementFactory(typeof(StackPanel));
                    elemFactory.AppendChild(elemborder);
                    ControlTemplate template = new ControlTemplate
                    {
                        VisualTree = elemFactory
                    };
                    DockPanel InfoPanel = new DockPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                    };
                    StackPanel bottomPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };
                    Label nameLabel = new Label
                    {
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Width = panel.Width - border.Width,
                        Content = Path.GetFileName(folder),
                        Foreground = new SolidColorBrush(fontscolor)
                    };
                    Image pageImage = new Image();
                    Label pageLabel = new Label
                    {
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Content = h.page.ToString() + "p",
                        Foreground = new SolidColorBrush(fontscolor)
                    };
                    bottomPanel.Children.Add(pageLabel);

                    InfoPanel.Children.Add(nameLabel);
                    DockPanel.SetDock(nameLabel, Dock.Top);
                    InfoPanel.Children.Add(bottomPanel);
                    DockPanel.SetDock(bottomPanel, Dock.Bottom);

                    ToolTip toolTip = new ToolTip
                    {
                        Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
                        Template = template
                    };
                    Image image = new Image
                    {
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Width = panel.Height,
                        Source = h.thumb,
                        OpacityMask = ib,
                        ToolTip = toolTip
                    };
                    image.MouseDown += (object sender, MouseButtonEventArgs e) =>
                    {
                        Reader reader = new Reader(h, this);
                        reader.Show();
                    };
                    border.Child = image;
                    panel.Children.Add(border);
                    panel.Children.Add(InfoPanel);
                    MainPanel.Children.Add(panel);
                }));
            }
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                label.Visibility = Visibility.Hidden;
            }));
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            background = Colors.Black;
            imagecolor = Colors.LightGray;
            Menuground = Color.FromRgb(33, 33, 33);
            MenuItmclr = Color.FromRgb(76, 76, 76);
            panelcolor = Color.FromRgb(76, 76, 76);
            fontscolor = Colors.White;
            outlineclr = Colors.White;
            this.Background = new SolidColorBrush(Menuground);
            MainMenuBackground.Color = Menuground;
            foreach (MenuItem menuItem in MainMenu.Items)
            {
                menuItem.Background = new SolidColorBrush(MenuItmclr);
                menuItem.Foreground = new SolidColorBrush(fontscolor);
                foreach (MenuItem item in menuItem.Items)
                    item.Foreground = new SolidColorBrush(Colors.Black);
            }
            foreach (Reader reader in Readers)
                reader.ChangeMode();
            LoadHitomi(Path.Combine(rootDir, folder));
        }

        public void SetMenuItemColor(ItemCollection items, Color Itemscolor)
        {
            foreach (Control menuItem in items)
            {
                menuItem.Background = new SolidColorBrush(Itemscolor);
                menuItem.Foreground = new SolidColorBrush(fontscolor);
                if (menuItem.GetType() == new MenuItem().GetType())
                    SetMenuItemColor(((MenuItem)menuItem).Items, Itemscolor);
            }
        }

        private void DarkMode_Unchecked(object sender, RoutedEventArgs e)
        {
            background = Colors.White;
            imagecolor = Colors.LightGray;
            Menuground = Color.FromRgb(240, 240, 240);
            MenuItmclr = Colors.White;
            panelcolor = Colors.White;
            fontscolor = Colors.Black;
            outlineclr = Colors.Black;
            //SetResourceReference(BackgroundProperty, background);
            MainMenuBackground.Color = Menuground;
            foreach (MenuItem menuItem in MainMenu.Items)
            {
                menuItem.Background = new SolidColorBrush(MenuItmclr);
                menuItem.Foreground = new SolidColorBrush(fontscolor);
                foreach (MenuItem item in menuItem.Items)
                    item.Foreground = new SolidColorBrush(Colors.Black);
            }
            foreach (Reader reader in Readers)
                reader.ChangeMode();
            LoadHitomi(Path.Combine(rootDir, folder));
        }

        private static BitmapFrame FastResize(BitmapFrame bfPhoto, int nWidth, int nHeight)
        {
            TransformedBitmap tbBitmap = new TransformedBitmap(bfPhoto, new ScaleTransform(nWidth / bfPhoto.Width, nHeight / bfPhoto.Height, 0, 0));
            return BitmapFrame.Create(tbBitmap);
        }

        public class InputBox
        {

            Window Box = new Window();//window for the inputbox
            FontFamily font = new FontFamily("Tahoma");//font for the whole inputbox
            int FontSize = 30;//fontsize for the input
            StackPanel sp1 = new StackPanel();// items container
            string title = "InputBox";//title as heading
            string boxcontent;//title
            string defaulttext = "Write here";//default textbox content
            string errormessage = "Invalid answer";//error messagebox content
            string errortitle = "Error";//error messagebox heading title
            string okbuttontext = "OK";//Ok button content
            Brush BoxBackgroundColor = Brushes.White;// Window Background
            Brush InputBackgroundColor = Brushes.White;// Textbox Background
            bool clicked = false;
            TextBox input = new TextBox();
            Button ok = new Button();
            bool inputreset = false;

            public InputBox(string content)
            {
                try
                {
                    boxcontent = content;
                }
                catch { boxcontent = "Error!"; }
                windowdef();
            }

            public InputBox(string content, string Htitle, string DefaultText)
            {
                try
                {
                    boxcontent = content;
                }
                catch { boxcontent = "Error!"; }
                try
                {
                    title = Htitle;
                }
                catch
                {
                    title = "Error!";
                }
                try
                {
                    defaulttext = DefaultText;
                }
                catch
                {
                    DefaultText = "Error!";
                }
                windowdef();
            }

            public InputBox(string content, string Htitle, string Font, int Fontsize)
            {
                try
                {
                    boxcontent = content;
                }
                catch { boxcontent = "Error!"; }
                try
                {
                    font = new FontFamily(Font);
                }
                catch { font = new FontFamily("Tahoma"); }
                try
                {
                    title = Htitle;
                }
                catch
                {
                    title = "Error!";
                }
                if (Fontsize >= 1)
                    FontSize = Fontsize;
                windowdef();
            }

            private void windowdef()// window building - check only for window size
            {
                Box.Height = 500;// Box Height
                Box.Width = 300;// Box Width
                Box.Background = BoxBackgroundColor;
                Box.Title = title;
                Box.Content = sp1;
                Box.Closing += Box_Closing;
                TextBlock content = new TextBlock();
                content.TextWrapping = TextWrapping.Wrap;
                content.Background = null;
                content.HorizontalAlignment = HorizontalAlignment.Center;
                content.Text = boxcontent;
                content.FontFamily = font;
                content.FontSize = FontSize;
                sp1.Children.Add(content);

                input.Background = InputBackgroundColor;
                input.FontFamily = font;
                input.FontSize = FontSize;
                input.HorizontalAlignment = HorizontalAlignment.Center;
                input.Text = defaulttext;
                input.MinWidth = 200;
                input.MouseEnter += input_MouseDown;
                sp1.Children.Add(input);
                ok.Width = 70;
                ok.Height = 30;
                ok.Click += ok_Click;
                ok.Content = okbuttontext;
                ok.HorizontalAlignment = HorizontalAlignment.Center;
                sp1.Children.Add(ok);

            }

            void Box_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            {
                if (!clicked)
                    e.Cancel = true;
            }

            private void input_MouseDown(object sender, MouseEventArgs e)
            {
                if ((sender as TextBox).Text == defaulttext && inputreset == false)
                {
                    (sender as TextBox).Text = null;
                    inputreset = true;
                }
            }

            void ok_Click(object sender, RoutedEventArgs e)
            {
                clicked = true;
                if (input.Text == defaulttext || input.Text == "")
                    MessageBox.Show(errormessage, errortitle);
                else
                {
                    Box.Close();
                }
                clicked = false;
            }

            public string ShowDialog()
            {
                Box.Width = 400;
                Box.Height = 200;
                Box.ShowDialog();
                return input.Text;
            }
        }

        private void SearchMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new TaskFactory().StartNew(() => LoadHitomi(Path.Combine(rootDir, folder)));
        }
    }

    public class Hitomi
    {
        public string name;
        public string dir;
        public int page;
        public string[] files;
        public BitmapImage thumb;
        public BitmapImage[] images;
    }

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

    }
}
