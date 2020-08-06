using HitomiViewer.Encryption;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace HitomiViewer.Scripts
{
    class FileEncrypt
    {
        public delegate byte[] DelegateEncrypt(byte[] data, string key);
        public static readonly DelegateEncrypt Encrypt = new DelegateEncrypt(AES128.Encrypt);

        public delegate bool DelegateTryEncrypt(ref byte[] byteDecrypt, byte[] byteToEncrypt, string key);
        public static readonly DelegateTryEncrypt TryEncrypt = new DelegateTryEncrypt(AES128.TryEncrypt);

        public static void AutoFe(string url)
        {
            if (Global.AutoFileEn)
            {
                Files(url);
            }
        }
        public static void Files(string url, string password = null)
        {
            password = password ?? FilePassword.Password;
            string[] files = Directory.GetFiles(url);
            foreach (string file in files)
            {
                if (Path.GetFileName(file) == "info.json") continue;
                if (Path.GetFileName(file) == "info.txt") continue;
                if (Path.GetExtension(file) == ".lock") continue;
                byte[] org = File.ReadAllBytes(file);
                byte[] enc = Encrypt(org, password);
                File.Delete(file);
                File.WriteAllBytes(file + ".lock", enc);
            }
        }
        public static void DownloadAsync(string url, string path) => DownloadAsync(new Uri(url), path);
        public static void DownloadAsync(Uri url, string path)
        {
            WebClient wc = new WebClient();
            wc.DownloadDataAsync(url, path);
            wc.DownloadDataCompleted += (object sender2, DownloadDataCompletedEventArgs e2)
                => File.WriteAllBytes(e2.UserState.ToString(), Encrypt(e2.Result, FilePassword.Password));
        }
    }
    class FileDecrypt
    {
        public static void Files(string url, string password = null)
        {
            password = password ?? FilePassword.Password;
            string[] files = Directory.GetFiles(url);
            foreach (string file in files)
            {
                try
                {
                    byte[] org = File.ReadAllBytes(file);
                    byte[] enc = AES128.Decrypt(org, password);
                    File.Delete(file);
                    File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)), enc);
                }
                catch { }
            }
        }
        public static bool TryFiles(string url, string password = null, string[] excepts = null, string[] allows = null)
        {
            password = password ?? Global.Password;
            excepts = excepts ?? new string[] { };
            bool err = false;
            string[] files = Directory.GetFiles(url);
            foreach (string file in files)
            {
                try
                {
                    if (excepts.Contains(Path.GetExtension(file))) continue;
                    if (allows != null && !allows.Contains(Path.GetExtension(file))) continue;
                    byte[] org = File.ReadAllBytes(file);
                    byte[] enc = AES128.Decrypt(org, password);
                    File.Delete(file);
                    File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)), enc);
                }
                catch { err = true; }
            }
            return !err;
        }
    }
    class FilePassword
    {
        public static string Password => Default(Global.OrginPassword);
        public static string Default(string org) => SHA256.Hash(org);
    }
}
