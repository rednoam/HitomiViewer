using HitomiViewer.Style;
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

namespace HitomiViewer
{
    /// <summary>
    /// HitomiPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HitomiPanel : UserControl
    {
        private Hitomi h;
        private BitmapImage thumb;
        private MainWindow MainWindow;
        public HitomiPanel(Hitomi h, MainWindow sender)
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
            else if (File.Exists(System.IO.Path.Combine(h.dir, "info.txt")))
            {
                HitomiInfoOrg hitomiInfoOrg = new HitomiInfoOrg();
                string[] lines = File.ReadAllLines(System.IO.Path.Combine(h.dir, "info.txt")).Where(x => x.Length > 0).ToArray();
                foreach (string line in lines)
                {
                    if (line.StartsWith("갤러리 넘버: "))
                    {

                    }
                    if (line.StartsWith("제목: "))
                    {

                    }
                    if (line.StartsWith("작가: "))
                    {

                    }
                    if (line.StartsWith("그룹: "))
                    {

                    }
                    if (line.StartsWith("타입: "))
                    {
                        hitomiInfoOrg.Types = line.Remove(0, "타입: ".Length);
                    }
                    if (line.StartsWith("시리즈: "))
                    {

                    }
                    if (line.StartsWith("캐릭터: "))
                    {

                    }
                    if (line.StartsWith("태그: "))
                    {
                        hitomiInfoOrg.Tags = line.Remove(0, "태그: ".Length);
                    }
                    if (line.StartsWith("언어: "))
                    {

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
            MainWindow.RemoveChild(this, h.dir);
        }

        private void Folder_Open_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(h.dir);
        }

        public class HitomiInfoOrg
        {
            public string Number { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string Group { get; set; }
            public string Types { get; set; }
            public string Series { get; set; }
            public string Character { get; set; }
            public string Tags { get; set; }
            public string Language { get; set; }
        }

        public class HitomiInfo
        {
            public static HitomiInfo Parse(HitomiInfoOrg org)
            {
                HitomiInfo info = new HitomiInfo();

                {
                    List<Tag> tags = new List<Tag>();
                    string[] arr = org.Tags.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in arr)
                    {
                        Tag tag = new Tag();
                        if (item.Contains(":"))
                        {
                            tag.types = (HitomiViewer.Tag.Types)Enum.Parse(typeof(HitomiViewer.Tag.Types), item.Split(':')[0]);
                            tag.name = string.Join(":", item.Split(':').Skip(1));
                        }
                        else
                        {
                            tag.types = HitomiViewer.Tag.Types.tag;
                            tag.name = item;
                        }

                        tags.Add(tag);
                    }
                    info.Tags = tags.ToArray();
                }
                return info;
            }
            public int Number { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string Group { get; set; }
            public Type Types { get; set; }
            public string Series { get; set; }
            public string Character { get; set; }
            public Tag[] Tags { get; set; }
            public string Language { get; set; }

            public enum Type
            {
                doujinshi,
                artistcg
            }

            public class Tag
            {
                public HitomiViewer.Tag.Types types { get; set; }
                public string name { get; set; }
            }
        }
    }
}
