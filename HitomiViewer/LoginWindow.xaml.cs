using HitomiViewer.Encryption;
using HitomiViewer.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;

namespace HitomiViewer
{
    /// <summary>
    /// LoginWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginWindow : Window
    {
        public delegate bool CheckDelegate(string password);
        public CheckDelegate CheckPassword;

        public string password = null;
        public int count = 3;

        public LoginWindow()
        {
            InitializeComponent();
            Password.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (password == null)
                DialogResult = true;
            if (SHA256.Hash(Password.Password) == password)
            {
                Global.OrginPassword = password;
                DialogResult = true;
            }
            else
            {
                count--;
                if (count <= 0) this.Close();
                else
                    MessageBox.Show($"비밀번호가 틀렸습니다. {count}회 남음");
            }
        }

        private void Password_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(sender, null);
            }
        }
    }
}
