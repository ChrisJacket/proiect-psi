using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using DataModel;
using Newtonsoft.Json;

namespace Communicator
{
    public static class HttpHelper
    {
        public static string BaseUrl { get; set; } = "http://localhost:5000";

        private static string Endpoint => BaseUrl.TrimEnd('/') + "/api/simulator";

        public static List<PumpSystemStatusEvent> GetDataFromWebAPI()
        {
            var request = (HttpWebRequest)WebRequest.Create(Endpoint);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            string html;
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<List<PumpSystemStatusEvent>>(html);
        }

        public static void PostDataToWebAPI(PumpSystemStatusEvent payload)
        {
            var request = WebRequest.Create(Endpoint);
            request.Method = "POST";
            string body = JsonConvert.SerializeObject(payload);
            byte[] bytes = Encoding.UTF8.GetBytes(body);
            request.ContentType = "application/json";
            request.ContentLength = bytes.Length;
            using (var dataStream = request.GetRequestStream())
            {
                dataStream.Write(bytes, 0, bytes.Length);
            }
            using (var response = request.GetResponse())
            using (var s = response.GetResponseStream())
            using (var reader = new StreamReader(s))
            {
                reader.ReadToEnd();
            }
        }

        public static void PostJsonToWebAPI(string json)
        {
            var request = WebRequest.Create(Endpoint);
            request.Method = "POST";
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            request.ContentType = "application/json";
            request.ContentLength = bytes.Length;
            using (var dataStream = request.GetRequestStream())
            {
                dataStream.Write(bytes, 0, bytes.Length);
            }
            using (var response = request.GetResponse())
            using (var s = response.GetResponseStream())
            using (var reader = new StreamReader(s))
            {
                reader.ReadToEnd();
            }
        }
    }
}
