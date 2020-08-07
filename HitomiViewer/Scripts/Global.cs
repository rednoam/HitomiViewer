using HitomiViewer.Scripts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HitomiViewer
{
    class Global
    {
        public static readonly string rootDir = AppDomain.CurrentDomain.BaseDirectory;

        public static MainWindow MainWindow;
        public const string basicPatturn = "*.jpg";
        public static Color background = Colors.White;
        public static Color Menuground = Color.FromRgb(240, 240, 240);
        public static Color MenuItmclr = Colors.White;
        public static Color childcolor = Colors.White;
        public static Color imagecolor = Colors.LightGray;
        public static Color panelcolor = Colors.White;
        public static Color fontscolor = Colors.Black;
        public static Color outlineclr = Colors.Black;
        public const int Magnif = 4;
        public const int RandomStringLength = 16;
        //[Obsolete("Password is deprecated, please use OriginPassword instead.", true)]
        public static string Password = null;
        public static string OrginPassword = null;
        public static string DownloadFolder = null;
        public static bool FileEn = false;
        public static bool AutoFileEn = false;
        public static bool EncryptTitle = false;
        public static bool RandomTitle = false;

        public class Config
        {
            public static readonly string path = Path.Combine(MainWindow.rootDir, "config.json");
            public static readonly string encryptpath = Path.Combine(MainWindow.rootDir, "config.lock");
        }
    }
}
