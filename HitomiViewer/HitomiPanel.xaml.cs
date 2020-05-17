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
            nameLabel.Content = System.IO.Path.GetFileName(h.dir);

            pageLabel.Content = h.page + "p";

            int GB = 1024 * 1024 * 1024;
            int MB = 1024 * 1024;
            int KB = 1024;
            DirectoryInfo info = new DirectoryInfo(h.dir);
            double FolderByte = info.EnumerateFiles().Sum(f => f.Length);
            sizeLabel.Content = Math.Round(FolderByte, 2) + "B";
            if (FolderByte > KB)
                sizeLabel.Content = Math.Round(FolderByte / KB, 2) + "KB";
            if (FolderByte > MB)
                sizeLabel.Content = Math.Round(FolderByte / MB, 2)+"MB";
            if (FolderByte > GB)
                sizeLabel.Content = Math.Round(FolderByte / GB, 2) + "GB";

            double SizePerPage = FolderByte / info.GetFiles().Length;
            sizeperpageLabel.Content = Math.Round(SizePerPage, 2) + "B";
            if (SizePerPage > KB)
                sizeperpageLabel.Content = Math.Round(SizePerPage / KB, 2) + "KB";
            if (SizePerPage > MB)
                sizeperpageLabel.Content = Math.Round(SizePerPage / MB, 2) + "MB";
            if (SizePerPage > GB)
                sizeperpageLabel.Content = Math.Round(SizePerPage / GB, 2) + "GB";

            ChangeColor(this);
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

            panel.Background = new SolidColorBrush(Global.background);
            border.Background = new SolidColorBrush(Global.imagecolor);
            InfoPanel.Background = new SolidColorBrush(Global.Menuground);
            bottomPanel.Background = new SolidColorBrush(Global.Menuground);
            nameLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            sizeLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            pageLabel.Foreground = new SolidColorBrush(Global.fontscolor);
        }

        private void Folder_Remove_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.RemoveChild(this, h.dir);
        }

        private void Folder_Open_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(h.dir);
        }
    }
}
