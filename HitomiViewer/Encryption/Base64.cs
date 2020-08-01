using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Encryption
{
    class Base64
    {
        public static string Encrypt(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        public static string Decrypt(string s) => Encoding.UTF8.GetString(Convert.FromBase64String(s));
    }
}
