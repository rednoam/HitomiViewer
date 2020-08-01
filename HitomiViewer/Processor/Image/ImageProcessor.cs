using ExtensionMethods;
using HitomiViewer.Encryption;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WebPWrapper;

namespace HitomiViewer.Scripts
{
    class ImageProcessor
    {
        public static BitmapImage ProcessEncrypt(string url)
        {
            if (url.isUrl())
            {
                if (url.EndsWith(".webp"))
                {
                    return LoadWebPImage(url);
                }
                else
                {
                    return LoadWebImage(url);
                }
            }
            else if (Global.FileEn)
            {
                try
                {
                    byte[] org = File.ReadAllBytes(url);
                    byte[] dec = AES128.Decrypt(org, Global.Password);
                    using (var ms = new MemoryStream(dec))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad; // here
                        image.StreamSource = ms;
                        image.EndInit();
                        return image;
                    }
                }
                catch {
                    try
                    {
                        return LoadMemory(url);
                    }
                    catch (FileNotFoundException)
                    {
                        return FromResource("NoImage.jpg");
                    }
                    catch (NotSupportedException)
                    {
                        return FromResource("ErrEncrypted.jpg");
                    }
                }
            }
            else
            {
                try
                {
                    return LoadMemory(url);
                }
                catch (FileNotFoundException)
                {
                    return FromResource("NoImage.jpg");
                }
                catch (NotSupportedException)
                {
                    return FromResource("ErrEncrypted.jpg");
                }
            }
        }
        public static async Task<BitmapImage> ProcessEncryptAsync(string url)
        {
            if (url.isUrl())
            {
                if (url.EndsWith(".webp"))
                    return await LoadWebPImageAsync(url);
                else
                    return await LoadWebImageAsync(url);
            }
            else if (Global.FileEn)
            {
                try
                {
                    byte[] org = File.ReadAllBytes(url);
                    byte[] dec = AES128.Decrypt(org, Global.Password);
                    using (var ms = new MemoryStream(dec))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad; // here
                        image.StreamSource = ms;
                        image.EndInit();
                        return image;
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        return LoadMemory(url);
                    }
                    catch (FileNotFoundException)
                    {
                        return FromResource("NoImage.jpg");
                    }
                    catch (NotSupportedException)
                    {
                        return FromResource("ErrEncrypted.jpg");
                    }
                }
            }
            else
            {
                try
                {
                    return LoadMemory(url);
                }
                catch (FileNotFoundException)
                {
                    return FromResource("NoImage.jpg");
                }
                catch (NotSupportedException)
                {
                    return FromResource("ErrEncrypted.jpg");
                }
            }
        }
        public static BitmapImage LoadMemory(string url)
        {
            try
            {
                var bitmap = new BitmapImage();
                var stream = File.OpenRead(url);

                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                stream.Close();
                stream.Dispose();
                bitmap.Freeze();
                return bitmap;
            }
            catch { return null; }
        }
        public static BitmapImage LoadWebImage(string url)
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
        public static async Task<BitmapImage> LoadWebImageAsync(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return null;
                System.Net.WebClient wc = new System.Net.WebClient();
                Byte[] MyData = await wc.DownloadDataTaskAsync(url);
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
        public static BitmapImage LoadWebPImage(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return null;
                System.Net.WebClient wc = new System.Net.WebClient();
                wc.Headers.Add("Referer", "https://hitomi.la/");
                Byte[] MyData = wc.DownloadData(url);
                wc.Dispose();
                WebP webP = new WebP();
                Bitmap bitmap = webP.Decode(MyData);
                MemoryStream ms = new MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                return bi;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<BitmapImage> LoadWebPImageAsync(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return null;
                System.Net.WebClient wc = new System.Net.WebClient();
                wc.Headers.Add("Referer", "https://hitomi.la/");
                Byte[] MyData = await wc.DownloadDataTaskAsync(url);
                wc.Dispose();
                WebP webP = new WebP();
                Bitmap bitmap = webP.Decode(MyData);
                MemoryStream ms = new MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                return bi;
            }
            catch
            {
                return null;
            }
        }
        public static BitmapImage FromResource(string psResourceName)
        {
            Uri oUri = new Uri($"pack://siteoforigin:,,,/Resources/{psResourceName}");
            return new BitmapImage(oUri);
        }
        public static BitmapImage FromResourceWithName(string psAssemblyName, string psResourceName)
        {
            Uri oUri = new Uri("pack://application:,,,/" + psAssemblyName + ";component/" + psResourceName, UriKind.RelativeOrAbsolute);
            return new BitmapImage(oUri);
        }
    }
}
