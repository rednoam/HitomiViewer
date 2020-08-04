using ExtensionMethods;
using HitomiViewer.Encryption;
using HitomiViewer.Scripts;
using HitomiViewer.Scripts.Loaders;
using HitomiViewer.Structs;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace HitomiViewer.UserControls
{
    /// <summary>
    /// HitomiPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HitomiPanel : UserControl
    {
        private Hitomi h;
        private BitmapImage thumb;
        private MainWindow MainWindow;
        private Hitomi.Type ftype = Hitomi.Type.Folder;
        private bool large;
        private bool afterLoad;
        private bool blur;
        public HitomiPanel(Hitomi h, MainWindow sender, bool large = false, bool afterLoad = false, bool blur = false)
        {
            this.large = large;
            this.afterLoad = afterLoad;
            this.blur = blur;
            this.h = h;
            this.thumb = h.thumb;
            this.MainWindow = sender;
            InitializeComponent();
            Init();
            InitEvent();
        }

        private void InitEvent()
        {
            thumbNail.MouseDown += (object sender, MouseButtonEventArgs e) =>
            {
                Reader reader = new Reader(h);
                if (!reader.IsClosed)
                    reader.Show();
            };
        }
        private void Init()
        {
            if (h.thumb == null)
                h.thumb = ImageProcessor.FromResource("NoImage.jpg");
            thumbNail.Source = h.thumb;
            thumbBrush.ImageSource = h.thumb;
            thumbNail.ToolTip = GetToolTip(panel.Height);

            authorsPanel.Children.Clear();
            authorsPanel.Children.Add(new Label { Content = "작가 :" });
            tagPanel.Children.Clear();
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
            if (blur)
                thumbNail.BitmapEffect = new BlurBitmapEffect { Radius = 5, KernelType = KernelType.Gaussian };
            Config config = new Config();
            config.Load();
            if (config.ArrayValue<string>(Settings.except_tags).Any(x => h.tags.Select(y => y.name).Contains(x)))
            {
                if (config.BoolValue(Settings.block_tags) ?? false)
                {
                    MainWindow.MainPanel.Children.Remove(this);
                    return;
                }
                else
                    thumbNail.BitmapEffect = new BlurBitmapEffect { Radius = 5, KernelType = KernelType.Gaussian };
            }
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.

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
            HitomiInfo hInfo = null;
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
                
                h.id = jobject.StringValue("id");
                h.name = jobject.StringValue("name");
                HitomiInfoOrg hInfoOrg = new HitomiInfoOrg();
                foreach (JToken tags in jobject["tags"])
                {
                    tag tag = new tag();
                    tag.TagType = (Tag.Types)int.Parse(tags["types"].ToString());
                    tag.TagName = tags["name"].ToString();
                    switch (tag.TagType)
                    {
                        case Structs.Tag.Types.female:
                            tag.TagColor = new SolidColorBrush(Color.FromRgb(255, 94, 94));
                            break;
                        case Structs.Tag.Types.male:
                            tag.TagColor = new SolidColorBrush(Color.FromRgb(65, 149, 244));
                            break;
                        case Structs.Tag.Types.tag:
                        default:
                            tag.TagColor = new SolidColorBrush(Color.FromRgb(153, 153, 153));
                            break;
                    }
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
                        hitomiInfoOrg.Tags = line.Remove(0, "태그: ".Length);
                    if (line.StartsWith("작가: "))
                        hitomiInfoOrg.Author = line.Remove(0, "작가: ".Length);
                    if (line.StartsWith("갤러리 넘버: "))
                        hitomiInfoOrg.Number = line.Remove(0, "갤러리 넘버: ".Length);
                    if (line.StartsWith("제목: "))
                        hitomiInfoOrg.Title = line.Remove(0, "제목: ".Length);
                }
                hInfo = HitomiInfo.Parse(hitomiInfoOrg);
                h.name = hInfo.Title;
                h.id = hInfo.Number.ToString();
                h.author = hInfo.Author;
                h.authors = hInfo.Author.Split(new string[] { ", " }, StringSplitOptions.None);
                foreach (Tag tag in hInfo.Tags)
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
            else
            {
                h.authors = new string[0];
            }

            if (large)
            {
                panel.Height = 150;
                authorsStackPanel.Visibility = Visibility.Visible;
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
            }

            nameLabel.Width = panel.Width - border.Width;
            nameLabel.Content = h.name;
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
            if (h.id == null || h.id == "")
            {
                DownloadData.Visibility = Visibility.Collapsed;
                DownloadImage.Visibility = Visibility.Collapsed;
            }
            if (ftype == Hitomi.Type.Hiyobi)
                Folder_Hiyobi_Search.Visibility = Visibility.Visible;
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
        public static void ChangeColor(HitomiPanel hpanel)
        {
            DockPanel panel = hpanel.panel as DockPanel;

            Border border = panel.Children[0] as Border;
            DockPanel InfoPanel = panel.Children[1] as DockPanel;

            StackPanel bottomPanel = InfoPanel.Children[1] as StackPanel;

            Label nameLabel = InfoPanel.Children[0] as Label;

            Label sizeLabel = bottomPanel.Children[0] as Label;
            Label pageLabel = bottomPanel.Children[2] as Label;

            StackPanel tagPanel = InfoPanel.Children[2] as StackPanel;

            panel.Background = new SolidColorBrush(Global.background);
            border.Background = new SolidColorBrush(Global.imagecolor);
            InfoPanel.Background = new SolidColorBrush(Global.Menuground);
            bottomPanel.Background = new SolidColorBrush(Global.Menuground);
            nameLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            sizeLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            pageLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            tagPanel.Background = new SolidColorBrush(Global.Menuground);
        }

        private async void HitomiPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (!afterLoad) return;
            if (h.type == Hitomi.Type.Hiyobi)
            {
                this.nameLabel.Content = h.name + " (로딩중)";
                h.files = new string[0];
                InternetP parser = new InternetP(index: int.Parse(h.id));
                this.h.files = (await parser.HiyobiFiles()).Select(x => x.url).ToArray();
                this.h.page = h.files.Length;
                this.nameLabel.Content = h.name;
                Init();
            }
            if (h.type == Hitomi.Type.Hitomi)
            {

            }
        }

        private void Folder_Remove_Click(object sender, RoutedEventArgs e)
        {
            Global.MainWindow.MainPanel.Children.Remove(this);
            Directory.Delete(h.dir, true);
        }
        private void Folder_Open_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(h.dir);
        }
        private void CopyNumber_Click(object sender, RoutedEventArgs e) => Clipboard.SetText(h.id);
        private void Hiyobi_Download_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                string filename = File2.GetDownloadTitle(File2.SaftyFileName(h.name));
                if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}"))
                    Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}");
                Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}");
                h.dir = $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}";
                h.Save($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/info.json");
                for (int i = 0; i < h.files.Length; i++)
                {
                    string file = h.files[i];
                    WebClient wc = new WebClient();
                    if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg"))
                    {
                        h.encrypted = Global.AutoFileEn;
                        if (Global.AutoFileEn)
                            FileEncrypt.DownloadAsync(new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg.lock");
                        else
                            wc.DownloadFileAsync(new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg");
                    }
                }
                Process.Start($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}");
            });
        }
        private void Hitomi_Download_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                string filename = File2.GetDownloadTitle(File2.SaftyFileName(h.name));
                if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}"))
                    Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}");
                Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}");
                h.dir = $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}";
                h.Save($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/info.json");
                for (int i = 0; i < h.files.Length; i++)
                {
                    string file = h.files[i];
                    WebClient wc = new WebClient();
                    wc.Headers.Add("referer", "https://hitomi.la/");
                    if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg"))
                    {
                        h.encrypted = Global.AutoFileEn;
                        if (Global.AutoFileEn)
                            FileEncrypt.DownloadAsync(new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg.lock");
                        else wc.DownloadFileAsync(new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg");
                    }
                }
                Process.Start($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}");
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
                        thumb = await ImageProcessor.LoadWebImageAsync($"https://cdn.hiyobi.me/tn/{tk["id"]}.jpg"),
                        type = Hitomi.Type.Hiyobi
                    };
                    Int64 size = 0;
                    h.files = imgs.ToList().Select(x => $"https://cdn.hiyobi.me/data/{tk["id"]}/{x["name"]}").ToArray();
                    h.FolderByte = size;
                    h.SizePerPage = size / imgs.Count;
                    foreach (JToken tags in tk["tags"])
                    {
                        Tag tag = new Tag();
                        if (tags["value"].ToString().Contains(":"))
                        {
                            tag.types = (Tag.Types)Enum.Parse(typeof(Tag.Types), tags["value"].ToString().Split(':')[0]);
                            tag.name = tags["display"].ToString();
                        }
                        else
                        {
                            tag.types = Structs.Tag.Types.tag;
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
            h.files = h.files.Select(x => x + ".lock").ToArray();
            h.encrypted = true;
            Process.Start(h.dir);
        }
        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            string[] files = Directory.GetFiles(h.dir);
            bool err = false;
            foreach (string file in files)
            {
                try
                {
                    byte[] org = File.ReadAllBytes(file);
                    byte[] enc = AES128.Decrypt(org, Global.Password);
                    File.Delete(file);
                    File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)), enc);
                }
                catch { err = true; }
            }
            if (err)
            {
                while (true)
                {
                    InputBox box = new InputBox("비밀번호를 입력해주세요.", "비밀번호가 맞지 않습니다.", "");
                    string password = box.ShowDialog();
                    if (box.canceled) return;
                    if (FileDecrypt.TryFiles(h.dir, SHA256.Hash(password), excepts: new string[] { ".txt", ".json" })) break;
                }
            }
            h.thumb = ImageProcessor.ProcessEncrypt(File2.GetImages(h.dir).First());
            h.files = h.files.Select(x => Path.Combine(Path.GetDirectoryName(x), Path.GetFileNameWithoutExtension(x))).ToArray();
            h.encrypted = false;
            thumbNail.Source = h.thumb;
            thumbNail.ToolTip = GetToolTip(panel.Height);
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
        private async void authorLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool result = await new InternetP(index: int.Parse(h.id)).isHiyobi();
            Label lbsender = sender as Label;
            string author = lbsender.Content.ToString();
            if (result)
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
            bool hiyobi = ftype == Hitomi.Type.Hiyobi;
            if (ftype == Hitomi.Type.Folder)
            {   
                bool result = await new InternetP(index: int.Parse(h.id)).isHiyobi();
                hiyobi = result;
            }
            if (hiyobi)
            {
                Hitomi h2 = await new HiyobiLoader(text: h.id).Parser();
                if (h2.name == "정보없음" && h2.id == "0")
                {
                    MessageBox.Show("데이터를 받아오는데 실패했습니다.");
                    return;
                }
                string folder = h.dir;
                if (h.type != Hitomi.Type.Folder)
                    folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Global.DownloadFolder, File2.GetDownloadTitle(File2.SaftyFileName(h2.name)));
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, "info.json"), JObject.FromObject(h2).ToString());
                MessageBox.Show("데이터를 성공적으로 받았습니다.");
                authorsPanel.Children.Clear();
                authorsPanel.Children.Add(new Label { Content = "작가 :" });
                tagPanel.Children.Clear();
                Init();
            }
            if (!hiyobi)
            {
                InternetP parser = new InternetP();
                parser.index = int.Parse(h.id);
                Hitomi h2 = await parser.HitomiData2();
                string folder = h.dir;
                if (h.type != Hitomi.Type.Folder)
                    folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Global.DownloadFolder, File2.GetDownloadTitle(File2.SaftyFileName(h2.name)));
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, "info.json"), JObject.FromObject(h2).ToString());
                MessageBox.Show("데이터를 성공적으로 받았습니다.");
                authorsPanel.Children.Clear();
                authorsPanel.Children.Add(new Label { Content = "작가 :" });
                tagPanel.Children.Clear();
                Init();
            }
        }
        private async void DownloadImage_Click(object sender, RoutedEventArgs e)
        {
            bool hiyobi = ftype == Hitomi.Type.Hiyobi;
            if (ftype == Hitomi.Type.Folder)
            {
                bool result = await new InternetP(index: int.Parse(h.id)).isHiyobi();
                hiyobi = result;
            }
            Hitomi h2 = null;
            if (hiyobi)
                h2 = await new HiyobiLoader(text: h.id).Parser();
            if (!hiyobi)
                h2 = await new InternetP(index: int.Parse(h.id)).HitomiData2();
            if (h2 == null) return;
            string folder = h.type == Hitomi.Type.Folder ? h.dir : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Global.DownloadFolder, File2.GetDownloadTitle(File2.SaftyFileName(h2.name)));
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder); h = h2;
            File.WriteAllText(Path.Combine(folder, "info.json"), JObject.FromObject(h2).ToString());
            foreach (string file in Directory.GetFiles(folder))
            {
                if (file.EndsWith(".lock") || file.EndsWith(".jpg"))
                    File.Delete(file);
            }
            for (int i = 0; i < h2.files.Length; i++)
            {
                string file = h2.files[i];
                WebClient wc = new WebClient();
                if (!hiyobi) wc.Headers.Add("referer", "https://hitomi.la/");
                h.encrypted = Global.AutoFileEn;
                if (Global.AutoFileEn)
                    FileEncrypt.DownloadAsync(new Uri(file), $"{folder}/{i}.jpg.lock");
                else
                    wc.DownloadFileAsync(new Uri(file), $"{folder}/{i}.jpg");
                if (i == 0) wc.DownloadFileCompleted += ImageDownloadCompleted;
            }
            Process.Start(folder);
        }
        private async void ImageDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            h.thumbpath = h.files.First();
            h.thumb = await ImageProcessor.ProcessEncryptAsync(h.thumbpath);
            Init();
        }
    }
}
