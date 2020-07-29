using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HitomiViewer.Scripts
{
    class CheckUpdate
    {
        public static async void Check()
        {
            Github github = new Github();
            github.owner = "rmagur1203";
            github.repos = "HitomiViewer";
            JArray jarray = await github.Releases();
            Version thisv = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Version repov = Version.Parse(jarray[0]["tag_name"].ToString());
            if (repov > thisv)
            {
                MessageBox.Show("업데이트가 필요합니다.");
                JToken item = jarray[0]["assets"].Where(x => x["browser_download_url"].ToString().EndsWith(".zip")).First();
                WebClient wc = new WebClient();
                if (File.Exists(Path.Combine(MainWindow.rootDir, "Update.zip")))
                    File.Delete(Path.Combine(MainWindow.rootDir, "Update.zip"));
                wc.DownloadFileAsync(new Uri(item["browser_download_url"].ToString()), Path.Combine(MainWindow.rootDir, "Update.zip"));
                wc.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                {
                    Process.Start(Path.Combine(MainWindow.rootDir, "Updater.exe"), "Update.zip");
                    Environment.Exit(0);
                };
            }
        }
    }
}
