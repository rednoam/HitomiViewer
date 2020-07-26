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
        public string data { get; set; }
        public List<string> keyword { get; set; }
        public int index { get; set; }

        public InternetP(string url = null, string data = null, List<string> keyword = null, int? index = null)
        {
            if (url != null) this.url = url;
            if (data != null) this.data = data;
            if (keyword != null) this.keyword = keyword;
            if (index != null) this.index = index.Value;
        }

        public async void LoadJObject(Action<JObject> callback)
        {
            callback(await LoadJObject());
        }
        public async Task<JObject> LoadJObject()
        {
            string html = await Load(url);
            return JObject.Parse(html);
        }
        public async void LoadJArray(Action<JArray> callback)
        {
            callback(await LoadJArray());
        }
        public async Task<JArray> LoadJArray()
        {
            string html = await Load(url);
            return JArray.Parse(html);
        }

        public async void TryLoadJObject(Action<JObject> callback)
        {
            callback(await TryLoadJObject());
        }
        public async Task<JObject> TryLoadJObject()
        {
            try
            {
                string html = await Load(url);
                return JObject.Parse(html);
            }
            catch
            {
                return null;
            }
        }
        public async void TryLoadJArray(Action<JArray> callback)
        {
            callback(await TryLoadJArray());
        }
        public async Task<JArray> TryLoadJArray()
        {
            try
            {
                string html = await Load(url);
                return JArray.Parse(html);
            }
            catch
            {
                return null;
            }
        }

        public void ParseJObject(Action<JObject> callback)
        {
            callback(ParseJObject());
        }
        public JObject ParseJObject()
        {
            return JObject.Parse(data);
        }
        public void ParseJArray(Action<JArray> callback)
        {
            callback(ParseJArray());
        }
        public JArray ParseJArray()
        {
            return JArray.Parse(data);
        }

        public async void HiyobiSearch(Action<string> callback)
        {
            callback(await HiyobiSearch());
        }
        public async Task<string> HiyobiSearch()
        {
            HttpClient client = new HttpClient();
            List<KeyValuePair<string, string>> body = new List<KeyValuePair<string, string>>();
            foreach (var key in this.keyword)
            {
                body.Add(new KeyValuePair<string, string>("search[]", key));
            }
            body.Add(new KeyValuePair<string, string>("paging", this.index.ToString()));
            var response = await client.PostAsync("https://api.hiyobi.me/search", new FormUrlEncodedContent(body));
            var pageContents = await response.Content.ReadAsStringAsync();
            return pageContents;
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
