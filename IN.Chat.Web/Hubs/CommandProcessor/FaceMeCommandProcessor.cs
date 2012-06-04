using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using IN.Chat.Web.Hubs.CommandProcessor.Core;
using IN.Chat.Web.Hubs.Entities;
using IN.Chat.Web.Hubs.Extensions;
using Newtonsoft.Json;

namespace IN.Chat.Web.Hubs.robotProcessor
{
    public class FaceMeCommandProcessor : ICommandProcessor
    {
        private static string[] Accessories { get { return new string[] { "hipster", "clown", "mustache", "scumbag", "jason" }; } }
        private const string REGEXFORMAT = @"(?i)(/{0}( ?me)? )(https?:(\S+)(png|jpg|jpeg))";
        private const string USAGEFORMAT = @"/{0} <image url>";
        private static List<Regex> Regexes { get; set; }
        public string Name { get { return "Face Me"; } }
        public string Description { get { return "Applies an item (random, hipster glasses, clown nose, mustache, scumbag steve hat, or Jason mask) to any face it finds."; } }
        public string[] Usage { get; private set; }

        public FaceMeCommandProcessor()
        {
            Regexes = new List<Regex>();
            Regexes.Add(new Regex(string.Format(REGEXFORMAT, "face")));
            Regexes.AddRange(Accessories.Select(a => new Regex(string.Format(REGEXFORMAT, a))));

            var usage = new List<string>();
            usage.Add(string.Format(USAGEFORMAT, "face"));
            usage.AddRange(Accessories.Select(a => string.Format(USAGEFORMAT, a)));
            Usage = usage.ToArray();
        }

        public bool CanProcess(Message message)
        {
            return Regexes.Any(r => r.IsMatch(message.ProcessedContent));
        }

        public void Process(Message message)
        {
            if (CanProcess(message))
            {
                var index = Regexes.FindIndex(r => r.IsMatch(message.ProcessedContent));
                var matches = Regexes.ElementAt(index).Matches(message.ProcessedContent);
                var accessory = index == 0 ? Accessories.RandomElement() : Accessories[index - 1];
                var urls = new List<string>();

                foreach (Match match in matches)
                {
                    urls.Add(match.Groups[3].Value);
                }

                urls = urls.Distinct().ToList();

                foreach (var url in urls)
                {
                    if (CanAccessorize(url))
                    {
                        message.ProcessedContent = String.Format("http://faceup.me/img.jpg?overlay={0}&src={1}", accessory, Uri.EscapeDataString(url));
                    }
                }
            }
        }

        protected virtual bool CanAccessorize(string imageUrl)
        {
            var canStache = false;

            var client = new HttpClient();
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-us"));
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            var result = client.GetAsync(String.Format("http://stacheable.herokuapp.com?src={0}", Uri.EscapeDataString(imageUrl))).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = result.Content.ReadAsStringAsync().Result;
                dynamic json = JsonConvert.DeserializeObject(content);
                canStache = json.count.Value > 0;
            }

            return canStache;
        }
    }
}