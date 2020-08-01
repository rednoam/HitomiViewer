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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

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
            else
            {
                config.Remove("pw");
            }
            cfg.Save(config);
            if (config["pw"] == null)
            {
                Global.MainWindow.Encrypt.Visibility = Visibility.Collapsed;
                Global.MainWindow.Decrypt.Visibility = Visibility.Collapsed;
            }
            else
            {
                Global.MainWindow.Encrypt.Visibility = Visibility.Visible;
                Global.MainWindow.Decrypt.Visibility = Visibility.Visible;
            }
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            Config cfg = new Config();
            JObject config = cfg.Load();
            List<string> decs = new List<string>();
            foreach (string item in Directory.GetDirectories(Global.MainWindow.path))
            {
                string[] files = Directory.GetFiles(item);
                foreach (string file in files)
                {
                    try
                    {
                        byte[] org = File.ReadAllBytes(file);
                        byte[] enc = AES128.Decrypt(org, Global.Password);
                        File.Delete(file);
                        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)), enc);
                        decs.Add(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)));
                    }
                    catch { }
                }
            }
            MessageBox.Show("복호화 완료");
            config["pw"] = SHA256.Hash(new InputBox("비밀번호를 입력해주세요.", "비밀번호 설정", "").ShowDialog());
            cfg.Save(config);
            foreach (string file in decs)
            {
                if (Path.GetFileName(file) == "info.json") continue;
                if (Path.GetFileName(file) == "info.txt") continue;
                if (Path.GetExtension(file) == ".lock") continue;
                byte[] org = File.ReadAllBytes(file);
                byte[] enc = AES128.Encrypt(org, Global.Password);
                File.Delete(file);
                File.WriteAllBytes(file + ".lock", enc);
            }
            MessageBox.Show("암호화 완료");
        }
    }
}
