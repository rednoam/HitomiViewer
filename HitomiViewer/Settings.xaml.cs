using ExtensionMethods;
using HitomiViewer.Encryption;
using HitomiViewer.Processor;
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
        private readonly string[] oldconfig = new string[] { "pw", "fe", "autofe", "et", "rt", "df", "fav" };
        private readonly string[] newconfig = new string[] { password, file_encrypt, download_file_encrypt, encrypt_title, random_title, download_folder, favorites };

        public const string password = "password";
        public const string file_encrypt = "file-encrypt";
        public const string download_file_encrypt = "download-file-encrypt";
        public const string encrypt_title = "encrypt-title";
        public const string random_title = "random-title";
        public const string download_folder = "download-file";
        public const string except_tags = "except-tags";
        public const string favorites = "favorites";
        public const string block_tags = "block_tags";

        private List<string> ExceptTagList = new List<string>();

        public Settings()
        {
            InitializeComponent();
            Config cfg = new Config();
            JObject config = cfg.Load();
            if (oldconfig.Where(x => config[x] != null).Count() > 0) Update();
            InitFolderName(config);
            InitPassword(config);
            InitEncrypt(config);
            InitTitle(config);
            InitTags(config);
            SafeData.IsChecked = (!File.Exists(Global.Config.path)) && File.Exists(Global.Config.encryptpath);
        }

        private void Update()
        {
            Config cfg = new Config();
            JObject oldconfig = cfg.Load();
            JObject newconfig = new JObject();
            string[] prev = this.oldconfig;
            string[] next = this.newconfig;
            for (int i = 0; i < prev.Length; i++)
            {
                newconfig[next[i]] = oldconfig[prev[i]];
            }
            cfg.Save(newconfig);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Config cfg = new Config();
            JObject config = cfg.Load();
            CheckPassword(ref config);
            CheckTitle(ref config);
            CheckTags(ref config);
            if (Global.OrginPassword == null)
                new LoginClass().Run();
            if (SafeData.IsChecked ?? false)
                Global.Password = FilePassword.Password;
            cfg.encrypt = SafeData.IsChecked ?? false;
            if (cfg.encrypt) File.Delete(Global.Config.path);

            cfg.Save(config);
            Close();
        }

        private void InitFolderName(JObject config)
        {
            FolderName.Content = config.StringValue(download_folder);
        }
        private void InitPassword(JObject config)
        {
            if (config.GetValue(password) != null)
                Password.IsChecked = true;
        }
        private void InitEncrypt(JObject config)
        {
            if (config.GetValue(file_encrypt) != null)
                FileEncrypt.IsChecked = config.BoolValue(download_file_encrypt);
            if (config.GetValue(download_file_encrypt) != null)
                AutoEncryption.IsChecked = config.BoolValue(download_file_encrypt);
        }
        private void InitTitle(JObject config)
        {
            if (config.GetValue(encrypt_title) != null)
                EncryptTitle.IsChecked = config.BoolValue(encrypt_title);
            if (config.GetValue(random_title) != null)
                RandomTitle.IsChecked = config.BoolValue(random_title);
        }
        private void InitTags(JObject config)
        {
            if (config[block_tags] != null)
            {
                BlockTags.IsChecked = config.BoolValue(block_tags);
            }
            if (config[except_tags] != null)
            {
                List<string> tags = config.ArrayValue<string>(except_tags).ToList();
                foreach (string tag in tags) ExceptTagList.Add(tag);
            }
            TagList2ListBox();
        }

        private void CheckPassword(ref JObject config)
        {
            if (Password.IsChecked.Value)
            {
                if (!config.ContainsKey(password))
                    config[password] = SHA256.Hash(new InputBox("비밀번호를 입력해주세요.", "비밀번호 설정", "").ShowDialog());
                config[file_encrypt] = FileEncrypt.IsChecked.Value;
                if (FileEncrypt.IsChecked.Value == true)
                    config[download_file_encrypt] = AutoEncryption.IsChecked.Value;
            }
            else
            {
                config.Remove(password);
            }
            if (config[password] == null)
            {
                Global.MainWindow.Encrypt.Visibility = Visibility.Collapsed;
                Global.MainWindow.Decrypt.Visibility = Visibility.Collapsed;
            }
            else
            {
                Global.MainWindow.Encrypt.Visibility = Visibility.Visible;
                Global.MainWindow.Decrypt.Visibility = Visibility.Visible;
            }
        }
        private void CheckTitle(ref JObject config)
        {
            config[encrypt_title] = EncryptTitle.IsChecked ?? false;
            if (!EncryptTitle.IsChecked ?? false)
                config[random_title] = RandomTitle.IsChecked ?? false;
        }
        private void CheckTags(ref JObject config)
        {
            config[block_tags] = BlockTags.IsChecked ?? false;
            if (ExceptTagList.Count > 0)
            {
                config[except_tags] = JToken.FromObject(ExceptTagList);
            }
        }

        private void TagList2ListBox()
        {
            ExceptTags.Items.Clear();
            foreach (string tag in ExceptTagList)
            {
                StackPanel stack = new StackPanel();
                stack.Children.Add(new Label
                {
                    MinWidth = 176,
                    Content = tag
                });
                Button btn = new Button { Content = "x" };
                btn.Click += (object sender, RoutedEventArgs e) =>
                {
                    ExceptTags.Items.Remove(stack);
                    ExceptTagList.Remove(tag);
                };
                stack.Children.Add(btn);
                ExceptTags.Items.Add(stack);
            }
        }

        private void Password_Checked(object sender, RoutedEventArgs e) => FileEncrypt.IsEnabled = true;
        private void Password_Unchecked(object sender, RoutedEventArgs e) => FileEncrypt.IsEnabled = false;
        private void FileEncrypt_Checked(object sender, RoutedEventArgs e) => AutoEncryption.IsEnabled = true;
        private void FileEncrypt_Unchecked(object sender, RoutedEventArgs e) => AutoEncryption.IsEnabled = false;
        private void RandomTitle_Checked(object sender, RoutedEventArgs e) => EncryptTitle.IsEnabled = false;
        private void RandomTitle_Unchecked(object sender, RoutedEventArgs e) => EncryptTitle.IsEnabled = true;
        private void EncryptTitle_Checked(object sender, RoutedEventArgs e) => RandomTitle.IsEnabled = false;
        private void EncryptTitle_Unchecked(object sender, RoutedEventArgs e) => RandomTitle.IsEnabled = true;
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
                        byte[] enc = FileDecrypt.Default(org);
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
                byte[] enc = Scripts.FileEncrypt.Default(org);
                File.Delete(file);
                File.WriteAllBytes(file + ".lock", enc);
            }
            MessageBox.Show("암호화 완료");
        }
        private void ChangeDownloadFolder_Click(object sender, RoutedEventArgs e)
        {
            Config cfg = new Config();
            JObject config = cfg.Load();
            config[download_folder] = new InputBox("다운로드 폴더 설정", "설정", "").ShowDialog();
            cfg.Save(config);
            FolderName.Content = config.StringValue(download_folder);
        }
        private void RandomDownloadFolder_Click(object sender, RoutedEventArgs e)
        {
            Config cfg = new Config();
            JObject config = cfg.Load();
            config[download_folder] = Random2.RandomString(int.Parse(new InputBox("랜덤 길이 지정", "설정", "").ShowDialog()));
            cfg.Save(config);
            FolderName.Content = config.StringValue(download_folder);
        }
        private void ExcecptTagsText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ExceptTagsBtn_Click(null, null);
        }
        private void ExceptTagsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!HiyobiTags.Tags.Select(x => x.name).Contains(ExceptTagsText.Text)) return;
            ExceptTagList.Add(ExceptTagsText.Text);
            ExceptTagsText.Text = "";
            TagList2ListBox();
        }
    }
}
