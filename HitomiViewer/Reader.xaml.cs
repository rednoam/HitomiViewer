using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Shapes;

namespace HitomiViewer
{
    /// <summary>
    /// Reader.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Reader : Window
    {
        private Hitomi hitomi;
        private MainWindow window;
        private int page;

        protected override void OnSourceInitialized(EventArgs e)
        {
            //IconHelper.RemoveIcon(this);
        }

        public Reader(Hitomi hitomi)
        {
            this.Background = new SolidColorBrush(Global.background);
            this.hitomi = hitomi.Copy();
            this.window = Global.MainWindow;
            this.page = 0;
            InitializeComponent();
            Init();
        }

        void Init()
        {
            this.window.Readers.Add(this);
            this.Closing += (object sender, System.ComponentModel.CancelEventArgs e) => window.Readers.Remove(this);
            this.image.Source = hitomi.thumb;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            this.hitomi.files = Directory.GetFiles(this.hitomi.dir).Where(file => allowedExtensions.Any(file.ToLower().EndsWith)).ToArray().ESort().ToArray();
            if (hitomi.thumb == null) this.image.Source = ImageSourceLoad(hitomi.files[0]);
            new TaskFactory().StartNew(() => {
                System.Threading.Thread.Sleep(100);
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    this.Activate();
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                }));
            });
        }

        public void ChangeMode()
        {
            this.Background = new SolidColorBrush(Global.background);
        }

        private void Image_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                if (page < hitomi.page - 1)
                {
                    page++;
                }
            }
            else if (e.Key == Key.Left)
            {
                if (page > 0)
                {
                    page--;
                }
            }
            if (e.Key == Key.Right || e.Key == Key.Left)
            {
                PreLoad();
                SetImage(hitomi.files[page]);
            }
            /*else if (e.Key == Key.Space)
            {
                image.Source = hitomi.images[page];
                TestImageBox imageBox = new TestImageBox();
                imageBox.image1.Source = new BitmapImage(new Uri(hitomi.files[page]));
                imageBox.Show();
                Clipboard.SetImage(new BitmapImage(new Uri(hitomi.files[page])));
                MessageBox.Show(hitomi.files[page]);
            }*/
            this.Title = hitomi.name;
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
            else if (e.Key == Key.F9)
            {
                this.hitomi = Hitomi.GetHitomi(hitomi.dir, Global.basicPatturn);
                this.hitomi.files = Directory.GetFiles(this.hitomi.dir, "*.jpg").ESort().ToArray();
                this.page = 0;
                this.image.Source = new BitmapImage(new Uri(this.hitomi.files[page]));
                MessageBox.Show("다시 로드됨.");
            }
            else if (e.Key == Key.Escape)
            {
                if (WindowStyle == WindowStyle.None && WindowState == WindowState.Maximized)
                {
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;
                }
            }
            else if (e.Key == Key.F8)
            {
                SetImage(new Uri(hitomi.files[page]));
                MessageBox.Show("(페이지만)다시 로드됨.");
            }
            else if (e.Key == Key.F7)
            {
                this.page = int.Parse(new InputBox("페이지: ").ShowDialog());
                MessageBox.Show("(페이지)"+this.page);
            }
            else if (e.Key == Key.R)
            {
                window.label.FontSize = 100;
                window.label.Content = "로딩중";
                window.label.Visibility = Visibility.Visible;
                this.Background = new SolidColorBrush(Global.background);
                window.MainPanel.Children.Clear();
                new TaskFactory().StartNew(() => window.LoadHitomi(window.path));
                string[] innerFiles = System.IO.Directory.GetFiles(hitomi.dir, "*.jpg").ESort().ToArray();
                hitomi = new Hitomi
                {
                    name = hitomi.dir.Split(System.IO.Path.DirectorySeparatorChar).Last(),
                    dir = hitomi.dir,
                    page = innerFiles.Length,
                    thumb = new BitmapImage(new Uri(innerFiles.First()))
                };
                SetImage(new Uri(hitomi.files[page]));
            }
            //MessageBox.Show(e.Key.ToString());
        }

        private void SetImage(Uri link)
        {
            var src = new BitmapImage();
            src.BeginInit();
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            src.DownloadFailed += delegate {
                Console.WriteLine("Failed");
            };

            src.DownloadProgress += delegate {
                Console.WriteLine("Progress");
            };

            src.DownloadCompleted += delegate {
                Console.WriteLine("Completed");
            };
            src.UriSource = link;
            src.EndInit();
            image.Source = src;
        }

        private void SetImage(string link)
        {
            image.Source = ImageSourceLoad(link);

            //Bitmap test = new Bitmap(link);
            //image.Source = ImageSourceFromBitmap(test);
        }

        private void PreLoad()
        {
            return;
            Console.WriteLine(page + "/" + ((page-1) % 10 == 0).ToString()+"/"+((page - 1) % 10).ToString());
            for (int i = page; (page-1) % 10 == 0 && i < page+10 && i < hitomi.page; i++)
            {
                Console.WriteLine(i.ToString());
                new BitmapImage(new Uri(this.hitomi.files[i]));
            }
        }

        private void Image_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //image.Source = new BitmapImage(new Uri("My%20Application;component/error-803716_960_720.png", UriKind.Relative));
                if (page < hitomi.page - 1)
                {
                    page++;
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                //image.Source = new BitmapImage(new Uri("My%20Application;component/error-803716_960_720.png", UriKind.Relative));
                if (page > 0)
                {
                    page--;
                }
            }
            if (e.RightButton == MouseButtonState.Pressed || e.LeftButton == MouseButtonState.Pressed)
            {
                PreLoad();
                SetImage(hitomi.files[page]);
            }
            this.Title = hitomi.name;
        }

        private string ImageSourceToString(ImageSource imageSource) {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;
            var encoder = new BmpBitmapEncoder();
            if (bitmapSource != null) {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                using (var stream = new System.IO.MemoryStream()) {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }
            return Convert.ToBase64String(bytes);
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            try
            {
                var handle = bmp.GetHbitmap();
                BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(handle);
                return source;
            }
            catch
            {
                return ImageSourceFromBitmap(bmp);
            }
        }

        private ImageSource ImageSourceLoad(string path, int pause = 0)
        {
            Console.WriteLine("{0}\n{1}", path, pause);
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.UriSource = new Uri(path);
            img.EndInit();

            System.Threading.Thread.Sleep(pause);

            if (img.PixelWidth == 1 && img.PixelHeight == 1) return ImageSourceLoad(path, pause + 500);

            return img;
        }
    }
}
