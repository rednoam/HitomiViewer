using ExtensionMethods;
using HitomiViewer.Encryption;
using HitomiViewer.Scripts;
using HitomiViewer.Scripts.Loaders;
using HitomiViewer.Structs;
using HitomiViewer.UserControls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using static HitomiViewer.UserControls.HitomiPanel;
using Label = System.Windows.Controls.Label;
using Path = System.IO.Path;
using ToolTip = System.Windows.Controls.ToolTip;

namespace HitomiViewer.UserControls
{
    /// <summary>
    /// HitomiPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LargeHitomiPanel : UserControl
    {
        private Hitomi h;
        private BitmapImage thumb;
        private MainWindow MainWindow;
        private Hitomi.Type ftype = Hitomi.Type.Folder;
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
            if (h.thumb == null)
            {
                h.thumb = new BitmapImage(new Uri("/Resources/NoImage.jpg", UriKind.Relative));
            }
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
            Uri uriResult;
            bool result = Uri.TryCreate(h.dir, UriKind.Absolute, out uriResult)
                && ((uriResult.Scheme == Uri.UriSchemeHttp) || (uriResult.Scheme == Uri.UriSchemeHttps));
            if (h.tags.Count > 0)
            {
                foreach (Tag tag in h.tags)
                {
                    tag tag1 = new tag
                    {
                        TagType = tag.types,
                        TagName = tag.name
                    };
                    switch (tag.types)
                    {
                        case Structs.Tag.Types.female:
                            tag1.TagColor = new SolidColorBrush(Color.FromRgb(255, 94, 94));
                            break;
                        case Structs.Tag.Types.male:
                            tag1.TagColor = new SolidColorBrush(Color.FromRgb(65, 149, 244));
                            break;
                        case Structs.Tag.Types.tag:
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
                h.id = jobject["id"].ToString();
                foreach (JToken tags in jobject["tags"])
                {
                    tag tag = new tag();
                    tag.TagType = (Tag.Types)int.Parse(tags["types"].ToString());
                    tag.TagName = tags["name"].ToString();
                    tagPanel.Children.Add(tag);
                }
                ftype = (Hitomi.Type)int.Parse(jobject["type"].ToString());
                if (jobject.ContainsKey("authors"))
                    h.authors = jobject["authors"].Select(x => x.ToString()).ToArray();
                else if (jobject.ContainsKey("author"))
                    h.authors = jobject["author"].ToString().Split(new string[] { ", " }, StringSplitOptions.None);
                else
                    h.authors = new string[0];
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
                    if (line.StartsWith("작가: "))
                    {
                        hitomiInfoOrg.Author = line.Remove(0, "작가: ".Length);
                    }
                }
                HitomiInfo Hinfo = HitomiInfo.Parse(hitomiInfoOrg);
                h.author = Hinfo.Author;
                h.authors = Hinfo.Author.Split(new string[] { ", " }, StringSplitOptions.None);
                foreach (Tag tag in Hinfo.Tags)
                {
                    tag tag1 = new tag
                    {
                        TagType = tag.types,
                        TagName = tag.name
                    };
                    switch (tag.types)
                    {
                        case Structs.Tag.Types.female:
                            tag1.TagColor = new SolidColorBrush(Color.FromRgb(255, 94, 94));
                            break;
                        case Structs.Tag.Types.male:
                            tag1.TagColor = new SolidColorBrush(Color.FromRgb(65, 149, 244));
                            break;
                        case Structs.Tag.Types.tag:
                        default:
                            tag1.TagColor = new SolidColorBrush(Color.FromRgb(153, 153, 153));
                            break;
                    }
                    tagPanel.Children.Add(tag1);
                }
            }
            if (h.authors == null) h.authors = new string[0];
            foreach (string artist in h.authors)
            {
                if (h.authors.ToList().IndexOf(artist) != 0)
                {
                    Label dot = new Label();
                    dot.Content = ", ";
                    dot.Padding = new Thickness(0, 5, 2.5, 5);
                    authorsPanel.Children.Add(dot);
                }
                Label lb = new Label();
                lb.Content = artist;
                lb.Foreground = new SolidColorBrush(Colors.Blue);
                lb.Cursor = Cursors.Hand;
                lb.MouseDown += authorLabel_MouseDown;
                lb.Padding = new Thickness(0, 5, 0, 5);
                authorsPanel.Children.Add(lb);
            }

            ContextSetup();
        }

        private void ContextSetup()
        {
            Favorite.Visibility = Visibility.Visible;
            FavoriteRemove.Visibility = Visibility.Collapsed;
            Folder_Remove.Visibility = Visibility.Collapsed;
            Folder_Hiyobi_Search.Visibility = Visibility.Collapsed;
            Hiyobi_Download.Visibility = Visibility.Collapsed;
            Hitomi_Download.Visibility = Visibility.Collapsed;
            Encrypt.Visibility = Visibility.Collapsed;
            Decrypt.Visibility = Visibility.Collapsed;
            DownloadData.Visibility = Visibility.Collapsed;
            switch (h.type)
            {
                case Hitomi.Type.Folder:
                    Folder_Remove.Visibility = Visibility.Visible;
                    if (Global.Password != null)
                    {
                        Encrypt.Visibility = Visibility.Visible;
                        Decrypt.Visibility = Visibility.Visible;
                    }
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
            if (ftype == Hitomi.Type.Hitomi || ftype == Hitomi.Type.Hiyobi)
                DownloadData.Visibility = Visibility.Visible;
            Config cfg = new Config();
            JObject obj = cfg.Load();
            List<string> favs = cfg.ArrayValue<string>("fav").ToList();
            if (favs.Contains(h.dir))
            {
                Favorite.Visibility = Visibility.Collapsed;
                FavoriteRemove.Visibility = Visibility.Visible;
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
            Global.MainWindow.MainPanel.Children.Remove(this);
            Directory.Delete(h.dir, true);
        }
        private void Folder_Open_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(h.dir);
        }
        private void Hiyobi_Download_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                string filename = h.name.Replace("|", "｜").Replace("?", "？");
                Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}");
                JObject jobject = JObject.FromObject(h);
                File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}/info.json", jobject.ToString());
                for (int i = 0; i < h.files.Length; i++)
                {
                    string file = h.files[i];
                    WebClient wc = new WebClient();
                    if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}/{i}.jpg"))
                    {
                        if (Global.AutoFileEn)
                        {
                            wc.DownloadDataAsync(new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}/{i}.jpg.lock");
                            wc.DownloadDataCompleted += (object sender2, DownloadDataCompletedEventArgs e2) =>
                            {
                                File.WriteAllBytes(e2.UserState.ToString(),
                                    AES128.Encrypt(e2.Result, Global.Password)); ;
                            };
                        }
                        else wc.DownloadFileAsync(new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}/{i}.jpg");
                    }
                }
                Process.Start($"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}");
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
                    {
                        if (Global.AutoFileEn)
                        {
                            wc.DownloadDataAsync(new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}/{i}.jpg.lock");
                            wc.DownloadDataCompleted += (object sender2, DownloadDataCompletedEventArgs e2) =>
                            {
                                File.WriteAllBytes(e2.UserState.ToString(),
                                    AES128.Encrypt(e2.Result, Global.Password)); ;
                            };
                        }
                        else wc.DownloadFileAsync(new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}/{i}.jpg");
                    }
                }
                Process.Start($"{AppDomain.CurrentDomain.BaseDirectory}/hitomi_downloaded/{filename}");
            });
        }
        private void Folder_Hiyobi_Search_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.MainPanel.Children.Clear();
            MainWindow.label.Visibility = Visibility.Visible;
            MainWindow.label.FontSize = 100;
            InternetP parser = new InternetP();
            HiyobiLoader loader = new HiyobiLoader();
            parser.keyword = h.name.Replace("｜", "|").Split(' ').ToList();
            parser.index = 1;
            HiyobiLoader hiyobi = new HiyobiLoader();
            hiyobi.Default();
            parser.HiyobiSearch(data => new InternetP(data: data).ParseJObject(loader.Parser));
        }
        private void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            string[] files = Directory.GetFiles(h.dir);
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
            Process.Start(h.dir);
        }
        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            string[] files = Directory.GetFiles(h.dir);
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
            Process.Start(h.dir);
        }
        private void Favorite_Click(object sender, RoutedEventArgs e)
        {
            Config cfg = new Config();
            JObject obj = cfg.Load();
            List<string> favs = cfg.ArrayValue<string>("fav").ToList();
            if (!favs.Contains(h.dir))
                favs.Add(h.dir);
            favs = favs.Where(x => Directory.Exists(x) || x.isUrl()).Distinct().ToList();
            obj["fav"] = JToken.FromObject(favs);
            cfg.Save(obj);
            ContextSetup();
        }
        private void FavoriteRemove_Click(object sender, RoutedEventArgs e)
        {
            Config cfg = new Config();
            JObject obj = cfg.Load();
            List<string> favs = cfg.ArrayValue<string>("fav").ToList();
            if (favs.Contains(h.dir))
                favs.Remove(h.dir);
            favs = favs.Where(x => Directory.Exists(x) || x.isUrl()).Distinct().ToList();
            obj["fav"] = JToken.FromObject(favs);
            cfg.Save(obj);
            ContextSetup();
        }
        private void authorLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("히요비에서 검색하시겠습니까?", "작가 검색", MessageBoxButton.YesNoCancel);
            Label lbsender = sender as Label;
            string author = lbsender.Content.ToString();
            if (result == MessageBoxResult.Yes)
            {
                Global.MainWindow.Hiyobi_Search_Text.Text = "artist:" + author;
                Global.MainWindow.Hiyobi_Search_Button_Click(this, null);
            }
            else
            {
                Global.MainWindow.Hitomi_Search_Text.Text = "artist:" + author;
                Global.MainWindow.Hitomi_Search_Button_Click(this, null);
            }
        }
        private async void DownloadData_Click(object sender, RoutedEventArgs e)
        {
            if (ftype == Hitomi.Type.Hitomi)
            {
                
            }
            if (ftype == Hitomi.Type.Hiyobi)
            {
                Hitomi h2 = await new HiyobiLoader(text: h.id).Parser();
                File.WriteAllText(Path.Combine(h.dir, "info.json"), JObject.FromObject(h2).ToString());
                MessageBox.Show("데이터를 성공적으로 받았습니다.");
                Init();
            }
        }
    }
}
