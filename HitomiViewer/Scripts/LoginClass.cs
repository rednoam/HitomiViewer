using HitomiViewer.Encryption;
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
        private byte[] BOrigin;
        private byte[] Decrypt;

        /// <summary>
        /// 로그인 실행
        /// </summary>
        public void Run()
        {
            if (!File.Exists(Global.Config.path) && File.Exists(Global.Config.encryptpath))
                Encrypted();
            else
                Plain();
        }
        /// <summary>
        /// 암호화된 json 파일로 로그인
        /// </summary>
        private void Encrypted()
        {
            BOrigin = File.ReadAllBytes(Global.Config.encryptpath);
            LoginWindow lw = new LoginWindow();
            lw.CheckPassword = CheckPassword1;
            if (lw.ShowDialog().Value)
            {
                try
                {
                    byte[] Decrypt = FileDecrypt.Decrypt(BOrigin, FilePassword.Default(lw.Password.Password));
                    string SOrigin = Encoding.UTF8.GetString(Decrypt);
                    Global.OrginPassword = lw.Password.Password;
                }
                catch { Environment.Exit(0); }
            }
        }
        /// <summary>
        /// 비암호화된 json 파일로 로그인
        /// </summary>
        private void Plain()
        {
            JObject config = new Config().Load();
            if (config == null) return;

            if (config[Settings.password] != null)
            {
                LoginWindow lw = new LoginWindow();
                lw.CheckPassword = CheckPassword2;
                if (!lw.ShowDialog().Value) Environment.Exit(0);
                Global.OrginPassword = lw.Password.Password;
            }
        }

        private bool CheckPassword2(string password)
        {
            return SHA256.Hash(password) == new Config().Load()[Settings.password].ToString();
        }

        private bool CheckPassword1(string password)
        {
            bool @try = FileDecrypt.TryDecrypt(ref Decrypt, BOrigin, FilePassword.Default(password));
            return @try;
        }
    }
}
