using ExtensionMethods;
using HitomiViewer.Scripts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using WebPWrapper;

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
        public bool IsClosed { get; private set; }

        protected override void OnSourceInitialized(EventArgs e)
        {
            //IconHelper.RemoveIcon(this);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        public Reader(Hitomi hitomi)
        {
            this.Background = new SolidColorBrush(Global.background);
            this.hitomi = hitomi;
            this.window = Global.MainWindow;
            this.page = 0;
            InitializeComponent();
            Init();
        }

        void Init()
        {
            this.window.Readers.Add(this);
            this.Loaded += (object sender, RoutedEventArgs e) => this.Focus();
            this.Closing += (object sender, System.ComponentModel.CancelEventArgs e) =>
            {
                window.Readers.Remove(this);
                hitomi.images = new BitmapImage[] { };
            };
            this.image.Source = hitomi.thumb;
            this.Title = hitomi.name;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (hitomi.files == null || hitomi.files.Length <= 0)
            {
                MessageBox.Show("이미지를 불러올 수 없습니다.");
                Close();
            }
            new TaskFactory().StartNew(() => {
                while (hitomi.files == null || hitomi.files.Length <= 0) { }
                if (hitomi.thumb == null) this.image.Source = ImageProcessor.ProcessEncrypt(hitomi.files[0]);
                System.Threading.Thread.Sleep(100);
                this.Activate();
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
            });
        }

        public void ChangeMode()
        {
            this.Background = new SolidColorBrush(Global.background);
        }

        private async void Image_KeyDown(object sender, KeyEventArgs e)
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
            else if (e.Key == Key.Enter)
            {
                Uri uriResult;
                bool result = Uri.TryCreate(hitomi.dir, UriKind.Absolute, out uriResult)
                    && ((uriResult.Scheme == Uri.UriSchemeHttp) || (uriResult.Scheme == Uri.UriSchemeHttps));
                if (result) {
                    try
                    {
                        this.Title = hitomi.name + " 0/" + (hitomi.files.Length - 1);
                        for (int i = 0; i < hitomi.files.Length; i++)
                        {
                            this.Title = hitomi.name + " " + i + "/" + (hitomi.files.Length - 1);
                            if (hitomi.images == null || hitomi.images.Length < hitomi.page)
                                hitomi.images = new BitmapImage[hitomi.page];
                            if (hitomi.images[i] == null)
                                hitomi.images[i] = await ImageProcessor.ProcessEncryptAsync(hitomi.files[i]);
                        }
                    }
                    catch { }
                }
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                Clipboard.SetImage((BitmapSource)this.image.Source);
            }
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

        private async void SetImage(string link)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(link, UriKind.Absolute, out uriResult)
                && ((uriResult.Scheme == Uri.UriSchemeHttp) || (uriResult.Scheme == Uri.UriSchemeHttps));
            image.Source = new BitmapImage(new Uri("/Resources/loading2.png", UriKind.Relative));
            int copypage = page;
            if (hitomi.images == null || hitomi.images.Length < hitomi.page)
                hitomi.images = new BitmapImage[hitomi.page];
            if (hitomi.images[copypage] == null)
                hitomi.images[copypage] = await ImageProcessor.ProcessEncryptAsync(link);
            if (copypage == page)
                image.Source = hitomi.images[page];
            /*
            if (result)
            {
                int copypage = page;
                if (hitomi.images == null)
                    hitomi.images = new BitmapImage[hitomi.page];
                if (hitomi.images[copypage] == null)
                {
                    if (link.EndsWith(".webp"))
                        hitomi.images[copypage] = await LoadWebP(link);
                    else
                        hitomi.images[copypage] = await LoadWebImageAsync(link);
                }
                if (copypage == page)
                    image.Source = hitomi.images[page];
            }
            if (!result)
                image.Source = ImageSourceLoad(link);
            */

            //Bitmap test = new Bitmap(link);
            //image.Source = ImageSourceFromBitmap(test);
        }
        private void Image_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (page < hitomi.page - 1)
                {
                    page++;
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                if (page > 0)
                {
                    page--;
                }
            }
            if (e.RightButton == MouseButtonState.Pressed || e.LeftButton == MouseButtonState.Pressed)
            {
                PreLoad();
                if (hitomi.files == null || hitomi.files.Length <= 0)
                    SetImage(new Uri("/Resources/loading2.png", UriKind.Relative));
                else
                    SetImage(hitomi.files[page]);
            }
        }
        private void PreLoad()
        {
            return;
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
    }
}
