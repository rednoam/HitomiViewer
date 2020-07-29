using HitomiViewer.Scripts;
using HitomiViewer.Style;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using static HitomiViewer.HitomiPanel;

namespace HitomiViewer
{
    /// <summary>
    /// HitomiPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LargeHitomiPanel : UserControl
    {
        private Hitomi h;
        private BitmapImage thumb;
        private MainWindow MainWindow;
        public LargeHitomiPanel(Hitomi h, MainWindow sender)
        {
            this.h = h;
            this.thumb = h.thumb;
            this.MainWindow = sender;
            InitializeComponent();
            InitEvent();
            Init();
        }

        private void InitEvent()
        {
            
        }

        private void Init()
        {
            thumbNail.Source = h.thumb;
            thumbBrush.ImageSource = h.thumb;
            thumbNail.ToolTip = GetToolTip(panel.Height);

            thumbNail.MouseDown += (object sender, MouseButtonEventArgs e) =>
            {
                Reader reader = new Reader(h);
                reader.Show();
            };

            nameLabel.Width = panel.Width - border.Width;
            nameLabel.Content = h.name;

            pageLabel.Content = h.page + "p";

            int GB = 1024 * 1024 * 1024;
            int MB = 1024 * 1024;
            int KB = 1024;
            double FolderByte = h.FolderByte;
            sizeLabel.Content = Math.Round(FolderByte, 2) + "B";
            if (FolderByte > KB)
                sizeLabel.Content = Math.Round(FolderByte / KB, 2) + "KB";
            if (FolderByte > MB)
                sizeLabel.Content = Math.Round(FolderByte / MB, 2) + "MB";
            if (FolderByte > GB)
                sizeLabel.Content = Math.Round(FolderByte / GB, 2) + "GB";

            double SizePerPage = h.SizePerPage;
            sizeperpageLabel.Content = Math.Round(SizePerPage, 2) + "B";
            if (SizePerPage > KB)
                sizeperpageLabel.Content = Math.Round(SizePerPage / KB, 2) + "KB";
            if (SizePerPage > MB)
                sizeperpageLabel.Content = Math.Round(SizePerPage / MB, 2) + "MB";
            if (SizePerPage > GB)
                sizeperpageLabel.Content = Math.Round(SizePerPage / GB, 2) + "GB";

            ChangeColor(this);
            Hitomi.Type ftype = Hitomi.Type.Folder;
            Uri uriResult;
            bool result = Uri.TryCreate(h.dir, UriKind.Absolute, out uriResult)
                && ((uriResult.Scheme == Uri.UriSchemeHttp) || (uriResult.Scheme == Uri.UriSchemeHttps));
            if (h.tags.Count > 0)
            {
                foreach (HitomiInfo.Tag tag in h.tags)
                {
                    tag tag1 = new tag
                    {
                        TagType = tag.types,
                        TagName = tag.name
                    };
                    switch (tag.types)
                    {
                        case HitomiViewer.Tag.Types.female:
                            tag1.TagColor = new SolidColorBrush(Color.FromRgb(255, 94, 94));
                            break;
                        case HitomiViewer.Tag.Types.male:
                            tag1.TagColor = new SolidColorBrush(Color.FromRgb(65, 149, 244));
                            break;
                        case HitomiViewer.Tag.Types.tag:
                        default:
                            tag1.TagColor = new SolidColorBrush(Color.FromRgb(153, 153, 153));
                            break;
                    }
                    tagPanel.Children.Add(tag1);
                }
            }
            else if (result)
            {

            }
            else if (File.Exists(System.IO.Path.Combine(h.dir, "info.json")))
            {
                JObject jobject = JObject.Parse(File.ReadAllText(System.IO.Path.Combine(h.dir, "info.json")));
                foreach (JToken tags in jobject["tags"])
                {
                    tag tag = new tag();
                    tag.TagType = (HitomiViewer.Tag.Types)int.Parse(tags["types"].ToString());
                    tag.TagName = tags["name"].ToString();
                    tagPanel.Children.Add(tag);
                }
                ftype = (Hitomi.Type)int.Parse(jobject["type"].ToString());
            }
            else if (File.Exists(System.IO.Path.Combine(h.dir, "info.txt")))
            {
                HitomiInfoOrg hitomiInfoOrg = new HitomiInfoOrg();
                string[] lines = File.ReadAllLines(System.IO.Path.Combine(h.dir, "info.txt")).Where(x => x.Length > 0).ToArray();
                foreach (string line in lines)
                {
                    if (line.StartsWith("태그: "))
                    {
                        hitomiInfoOrg.Tags = line.Remove(0, "태그: ".Length);
                    }
                }
                HitomiInfo Hinfo = HitomiInfo.Parse(hitomiInfoOrg);
                foreach (HitomiInfo.Tag tag in Hinfo.Tags)
                {
                    tag tag1 = new tag
                    {
                        TagType = tag.types,
                        TagName = tag.name
                    };
                    switch (tag.types)
                    {
                        case HitomiViewer.Tag.Types.female:
                            tag1.TagColor = new SolidColorBrush(Color.FromRgb(255, 94, 94));
                            break;
                        case HitomiViewer.Tag.Types.male:
                            tag1.TagColor = new SolidColorBrush(Color.FromRgb(65, 149, 244));
                            break;
                        case HitomiViewer.Tag.Types.tag:
                        default:
                            tag1.TagColor = new SolidColorBrush(Color.FromRgb(153, 153, 153));
                            break;
                    }
                    tagPanel.Children.Add(tag1);
                }
            }

            //Folder_Open.Visibility = Visibility.Collapsed;
            Folder_Remove.Visibility = Visibility.Collapsed;
            Folder_Hiyobi_Search.Visibility = Visibility.Collapsed;
            Hiyobi_Download.Visibility = Visibility.Collapsed;
            Hitomi_Download.Visibility = Visibility.Collapsed;
            switch (h.type)
            {
                case Hitomi.Type.Folder:
                    Folder_Remove.Visibility = Visibility.Visible;
                    break;
                case Hitomi.Type.Hiyobi:
                    Hiyobi_Download.Visibility = Visibility.Visible;
                    break;
                case Hitomi.Type.Hitomi:
                    Hitomi_Download.Visibility = Visibility.Visible;
                    break;
            }
            switch (ftype)
            {
                case Hitomi.Type.Hiyobi:
                    Folder_Hiyobi_Search.Visibility = Visibility.Visible;
                    break;
            }
        }

        private ToolTip GetToolTip(double height)
        {
            double b = height / h.thumb.Width;
            FrameworkElementFactory image = new FrameworkElementFactory(typeof(Image));
            {
                image.SetValue(Image.WidthProperty, b * h.thumb.Width * Global.Magnif);
                image.SetValue(Image.HeightProperty, b * h.thumb.Height * Global.Magnif);
                image.SetValue(Image.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                image.SetValue(Image.SourceProperty, h.thumb);
            }
            FrameworkElementFactory elemborder = new FrameworkElementFactory(typeof(Border));
            {
                elemborder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
                elemborder.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Global.outlineclr));
                elemborder.AppendChild(image);
            }
            FrameworkElementFactory panel = new FrameworkElementFactory(typeof(StackPanel));
            {
                panel.AppendChild(elemborder);
            }
            ToolTip toolTip = new ToolTip
            {
                Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
                Template = new ControlTemplate
                {
                    VisualTree = panel
                }
            };
            return toolTip;
        }

        public static void ChangeColor(LargeHitomiPanel hpanel)
        {
            DockPanel panel = hpanel.panel as DockPanel;

            Border border = panel.Children[0] as Border;
            DockPanel InfoPanel = panel.Children[1] as DockPanel;

            StackPanel bottomPanel = InfoPanel.Children[1] as StackPanel;
            //ScrollViewer scrollViewer = InfoPanel.Children[2] as ScrollViewer;

            Label nameLabel = InfoPanel.Children[0] as Label;

            Label sizeLabel = bottomPanel.Children[0] as Label;
            Label pageLabel = bottomPanel.Children[2] as Label;

            StackPanel tagPanel = InfoPanel.Children[2] as StackPanel;

            panel.Background = new SolidColorBrush(Global.background);
            border.Background = new SolidColorBrush(Global.imagecolor);
            InfoPanel.Background = new SolidColorBrush(Global.Menuground);
            bottomPanel.Background = new SolidColorBrush(Global.Menuground);
            //scrollViewer.Background = new SolidColorBrush(Global.Menuground);
            nameLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            sizeLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            pageLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            tagPanel.Background = new SolidColorBrush(Global.Menuground);
        }

        private void Folder_Remove_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow.RemoveChild(this, h.dir);
        }
        private void Folder_Open_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(h.dir);
        }
        private void Hiyobi_Download_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                string filename = h.name.Replace("|", "｜");
                Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}");
                JObject jobject = JObject.FromObject(h);
                File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}/info.json", jobject.ToString());
                for (int i = 0; i < h.files.Length; i++)
                {
                    string file = h.files[i];
                    WebClient wc = new WebClient();
                    if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}/{i}.jpg"))
                        wc.DownloadFileAsync(new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}/{i}.jpg");
                }
                System.Diagnostics.Process.Start($"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}");
            });
        }
        private void Hitomi_Download_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                string filename = h.name.Replace("|", "｜");
                Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}");
                JObject jobject = JObject.FromObject(h);
                File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}/info.json", jobject.ToString());
                for (int i = 0; i < h.files.Length; i++)
                {
                    string file = h.files[i];
                    WebClient wc = new WebClient();
                    wc.Headers.Add("referer", "https://hitomi.la/");
                    if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}/{i}.jpg"))
                        wc.DownloadFileAsync(new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}/{i}.jpg");
                }
                System.Diagnostics.Process.Start($"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}");
            });
        }
        private void Folder_Hiyobi_Search_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.MainPanel.Children.Clear();
            MainWindow.label.Visibility = Visibility.Visible;
            MainWindow.label.FontSize = 100;
            InternetP parser = new InternetP();
            parser.keyword = h.name.Replace("｜", "|").Split(' ').ToList();
            parser.index = 1;
            parser.HiyobiSearch(data => new InternetP(data: data).ParseJObject(async jobject =>
            {
                MainWindow.label.Content = 0 + "/" + jobject["list"].ToList().Count;
                foreach (JToken tk in jobject["list"])
                {
                    MainWindow.label.Content = jobject["list"].ToList().IndexOf(tk) + "/" + jobject["list"].ToList().Count;
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
                        thumbpath = $"https://cdn.hiyobi.me/tn/{tk["id"]}.jpg",
                        thumb = MainWindow.LoadImage($"https://cdn.hiyobi.me/tn/{tk["id"]}.jpg"),
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
                    MainWindow.MainPanel.Children.Add(new HitomiPanel(h, MainWindow));
                    Console.WriteLine($"Completed: https://cdn.hiyobi.me/tn/{tk["id"]}.jpg");
                }
                MainWindow.label.Visibility = Visibility.Hidden;
            }));
        }
    }
}
