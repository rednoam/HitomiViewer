using HitomiViewer.Scripts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HitomiViewer
{
    class Global
    {
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
        public static Config cfg = new Config();
        public static JObject cfgob = cfg.Load();
        public static string Password = cfg.StringValue(Settings.password);
        public static string DownloadFolder = cfg.StringValue(Settings.download_folder) ?? "hitomi_downloaded";
        public static bool FileEn = cfg.BoolValue(Settings.file_encrypt) ?? false;
        public static bool AutoFileEn = cfg.BoolValue(Settings.download_file_encrypt) ?? false;
        public static bool EncryptTitle = cfg.BoolValue(Settings.encrypt_title) ?? false;
        public static bool RandomTitle = cfg.BoolValue(Settings.random_title) ?? false;
    }
}
