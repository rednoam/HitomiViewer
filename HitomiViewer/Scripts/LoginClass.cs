using HitomiViewer.Scripts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HitomiViewer.Scripts
{
    class LoginClass
    {
        public void Test()
        {
            if (!File.Exists(Config.path) && File.Exists(Config.encryptpath))
            {
                byte[] BOrigin = File.ReadAllBytes(Config.encryptpath);
                byte[] Decrypt = null;
                bool @try = FileEncrypt.TryEncrypt(ref Decrypt, BOrigin, FilePassword.Password);
                string SOrigin = Encoding.UTF8.GetString(Decrypt);
            }
            else
            {
                JObject config = new Config().Load();
                if (config == null) return;

                if (config[Settings.password] != null)
                {
                    LoginWindow lw = new LoginWindow();
                    lw.password = config[Settings.password].ToString();
                    if (!lw.ShowDialog().Value) Environment.Exit(0);
                }
            }

            if (Global.FileEn && !File.Exists(Global.EncryptInfoFile))
            {
                MessageBox.Show("암호화 정보 파일이 없습니다.\n복호화가 불가능할 수 있습니다.");
            }
        }
    }
}
