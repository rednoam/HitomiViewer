using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HitomiViewer.Scripts
{
    class InternetP
    {
        public string url { get; set; }
        public async void ParseHiyobi(Action<JObject> callback)
        {
            callback(await ParseHiyobi());
        }
        public async Task<JObject> ParseHiyobi()
        {
            string html = await Load(url);
            return JObject.Parse(html);
        }
        public async Task<JArray> ParseJArray()
        {
            string html = await Load(url);
            return JArray.Parse(html);
        }
        public async Task<string> Load(string Url)
        {
            if (Url.Last() == '/') Url = Url.Remove(Url.Length - 1);
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(Url);
            var pageContents = await response.Content.ReadAsStringAsync();
            return pageContents;
        }
    }
}
