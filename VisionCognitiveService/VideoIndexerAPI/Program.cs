using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;

namespace VideoIndexerAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            var apiUrl = "https://videobreakdown.azure-api.net/Breakdowns/Api/Partner/Breakdowns";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "1ce0248108fa45118f24fb5ccd3f7536");

            var content = new MultipartFormDataContent();

            Console.WriteLine("Uploading...");
            var videoUrl = "https://sec.ch9.ms/ch9/b581/490ef9a8-6e04-46c7-a12b-0fb9fc86b581/HappyBirthdaydotNETwithDanFernandez.mp4";
            var result = client.PostAsync(apiUrl + "?name=some_name&description=some_description&privacy=private&partition=some_partition&videoUrl=" + videoUrl, content).Result;
            var json = result.Content.ReadAsStringAsync().Result;

            Console.WriteLine();
            Console.WriteLine("Uploaded:");
            Console.WriteLine(json);
             
            var id = JsonConvert.DeserializeObject<string>(json);
            string JSONtext = "";

            while (true)
            {
                Thread.Sleep(10000);

                result = client.GetAsync(string.Format(apiUrl + "/{0}/State", id)).Result;
                json = result.Content.ReadAsStringAsync().Result;
                
                Console.WriteLine();
                Console.WriteLine("State:");
                Console.WriteLine(json);
                JSONtext += json;

                dynamic state = JsonConvert.DeserializeObject(json);
                if (state.state != "Uploaded" && state.state != "Processing")
                {
                    break;
                }
            }

            result = client.GetAsync(string.Format(apiUrl + "/{0}", id)).Result;
            json = result.Content.ReadAsStringAsync().Result;
            Console.WriteLine();
            Console.WriteLine("Full JSON:");
            Console.WriteLine(json);
            JSONtext += json;

            result = client.GetAsync(string.Format(apiUrl + "/Search?id={0}", id)).Result;
            json = result.Content.ReadAsStringAsync().Result;
            Console.WriteLine();
            Console.WriteLine("Search:");
            Console.WriteLine(json);
            JSONtext += json;

            result = client.GetAsync(string.Format(apiUrl + "/{0}/InsightsWidgetUrl", id)).Result;
            json = result.Content.ReadAsStringAsync().Result;
            Console.WriteLine();
            Console.WriteLine("Insights Widget url:");
            Console.WriteLine(json);
            JSONtext += json;

            result = client.GetAsync(string.Format(apiUrl + "/{0}/PlayerWidgetUrl", id)).Result;
            json = result.Content.ReadAsStringAsync().Result;
            Console.WriteLine();
            Console.WriteLine("Player token:");
            Console.WriteLine(json);

            JSONtext += json;
            JSONtext = JsonPrettyPrint(JSONtext);

            Console.WriteLine(JSONtext);           

            Console.ReadLine();
        }

        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            string INDENT_STRING = "    ";
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < json.Length; i++)
            {
                var ch = json[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && json[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }


    }

    static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
            {
                action(i);
            }
        }
    }
}
