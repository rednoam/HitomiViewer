using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Scripts
{
    class Github
    {
        public string owner { get; set; }
        public string repos { get; set; }
        public async Task<JArray> Releases()
        {
            return JArray.Parse(await Load($"https://api.github.com/repos/{owner}/{repos}/releases"));
        }
        public async Task<JObject> ReleasesLatest()
        {
            return JObject.Parse(await Load($"https://api.github.com/repos/{owner}/{repos}/releases/latest"));
        }
        public async Task<string> Load(string Url)
        {
            if (Url.Last() == '/') Url = Url.Remove(Url.Length - 1);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"{repos}");
            var response = await client.GetAsync(Url);
            var pageContents = await response.Content.ReadAsStringAsync();
            return pageContents;
        }
    }
}
