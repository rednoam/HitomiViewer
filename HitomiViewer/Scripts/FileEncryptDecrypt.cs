using HitomiViewer.Encryption;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Scripts
{
    class FileEncrypt
    {
        public static void AutoFe(string url)
        {
            if (Global.AutoFileEn)
            {
                Files(url);
            }
        }
        public static void Files(string url)
        {
            string[] files = Directory.GetFiles(url);
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
        }
    }
    class FileDecrypt
    {
        public static void Files(string url)
        {
            string[] files = Directory.GetFiles(url);
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
        }
    }
}
