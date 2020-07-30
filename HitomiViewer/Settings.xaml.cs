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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Config cfg = new Config();
            JObject config = cfg.Load();
            if (Password.IsChecked.Value)
            {
                config["pw"] = SHA256.Hash(new InputBox("비밀번호를 입력해주세요.", "비밀번호 설정", "").ShowDialog());
            }
            cfg.Save(config);
            Close();
        }
    }
}
