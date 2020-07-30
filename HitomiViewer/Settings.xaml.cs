using HitomiViewer.Encryption;
using HitomiViewer.Scripts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HitomiViewer
{
    /// <summary>
    /// Settings.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            Config cfg = new Config();
            JObject config = cfg.Load();
            FileEncrypt.IsEnabled = false;
            AutoEncryption.IsEnabled = false;
            if (config.GetValue("pw") != null)
            {
                Password.IsChecked = true;
            }
            if (config.GetValue("fe") != null)
            {
                FileEncrypt.IsChecked = bool.Parse(config["fe"].ToString());
            }
            if (config.GetValue("autofe") != null)
            {
                AutoEncryption.IsChecked = bool.Parse(config["autofe"].ToString());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Config cfg = new Config();
            JObject config = cfg.Load();
            config["fe"] = false;
            config["autofe"] = false;
            if (Password.IsChecked.Value)
            {
                if (!config.ContainsKey("pw"))
                    config["pw"] = SHA256.Hash(new InputBox("비밀번호를 입력해주세요.", "비밀번호 설정", "").ShowDialog());
                config["fe"] = FileEncrypt.IsChecked.Value;
                if (FileEncrypt.IsChecked.Value == true)
                    config["autofe"] = AutoEncryption.IsChecked.Value;
            }
            cfg.Save(config);
            Close();
        }

        private void Password_Checked(object sender, RoutedEventArgs e)
        {
            FileEncrypt.IsEnabled = true;
        }
        private void Password_Unchecked(object sender, RoutedEventArgs e)
        {
            FileEncrypt.IsEnabled = false;
        }
        private void FileEncrypt_Checked(object sender, RoutedEventArgs e)
        {
            AutoEncryption.IsEnabled = true;
        }
        private void FileEncrypt_Unchecked(object sender, RoutedEventArgs e)
        {
            AutoEncryption.IsEnabled = false;
        }
    }
}
