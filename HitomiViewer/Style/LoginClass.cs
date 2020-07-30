using HitomiViewer.Scripts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Style
{
    class LoginClass
    {
        public void Test()
        {
            JObject config = new Config().Load();
            if (config == null) return;

            if (config["pw"] != null)
            {
                LoginWindow lw = new LoginWindow();
                lw.password = config["pw"].ToString();
                if (!lw.ShowDialog().Value) Environment.Exit(0);
            }
        }
    }
}
