using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Scripts
{
    class Config
    {
        private readonly string path = Global.Config.path;
        private readonly string encryptpath = Global.Config.encryptpath;
        public bool encrypt = false;
        private JObject config;

        public JObject Load()
        {
            if (!File.Exists(path))
                return EncryptLoad();
            config = JObject.Parse(File.ReadAllText(Global.Config.path));
            return config;
        }

        public JObject EncryptLoad()
        {
            if (File.Exists(path))
                return Load();
            if (!File.Exists(encryptpath)) return new JObject();
            byte[] BOrigin = File.ReadAllBytes(encryptpath);
            byte[] Decrypt = FileDecrypt.Decrypt(BOrigin, FilePassword.Password);
            string SOrigin = Encoding.UTF8.GetString(Decrypt);
            return JObject.Parse(SOrigin);
        }

        public string StringValue(string path)
        {
            if (config == null) return null;
            if(!config.ContainsKey(path)) return null;
            return config[path].ToString();
        }
        public bool? BoolValue(string path)
        {
            if (config == null) return null;
            if (!config.ContainsKey(path)) return null;
            return bool.Parse(config[path].ToString());
        }
        public IList<T> ArrayValue<T>(string path) where T : class
        {
            if (config == null) return new List<T>();
            if (!config.ContainsKey(path)) return new List<T>();
            return config[path].ToObject<List<T>>();
        }

        public bool Save(JObject data)
        {
            this.config = data;
            Global.DownloadFolder = StringValue(Settings.download_folder) ?? "hitomi_downloaded";
            Global.FileEn = BoolValue(Settings.file_encrypt) ?? false;
            Global.AutoFileEn = BoolValue(Settings.download_file_encrypt) ?? false;
            Global.EncryptTitle = BoolValue(Settings.encrypt_title) ?? false;
            Global.RandomTitle = BoolValue(Settings.random_title) ?? false;
            if (Global.DownloadFolder == "") Global.DownloadFolder = "hitomi_downloaded";
            string path = encrypt ? Global.Config.encryptpath : Global.Config.path;
            byte[] bytes = Encoding.UTF8.GetBytes(data.ToString());
            if (encrypt)
                bytes = FileEncrypt.Encrypt(bytes, Global.Password);
            File.WriteAllBytes(path, bytes);
            return true;
        }
    }
}
