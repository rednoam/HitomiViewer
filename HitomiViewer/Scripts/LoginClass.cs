using HitomiViewer.Scripts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Scripts
{
    class LoginClass
    {
        public void Test()
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
    }
}
