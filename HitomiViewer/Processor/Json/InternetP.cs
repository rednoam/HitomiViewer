using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Processor
{
    partial class InternetP
    {
        public async void LoadJObject(Action<JObject> callback) => callback(await LoadJObject());
        public async Task<JObject> LoadJObject()
        {
            string html = await Load(url);
            return JObject.Parse(html);
        }
        public async void LoadJArray(Action<JArray> callback) => callback(await LoadJArray());
        public async Task<JArray> LoadJArray()
        {
            string html = await Load(url);
            return JArray.Parse(html);
        }

        public async void TryLoadJObject(Action<JObject> callback) => callback(await TryLoadJObject());
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
        public async void TryLoadJArray(Action<JArray> callback) => callback(await TryLoadJArray());
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

        public void ParseJObject(Action<JObject> callback) => callback(ParseJObject());
        public JObject ParseJObject()
        {
            return JObject.Parse(data);
        }
        public void ParseJArray(Action<JArray> callback) => callback(ParseJArray());
        public JArray ParseJArray()
        {
            return JArray.Parse(data);
        }
    }
}
