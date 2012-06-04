using System;
using System.Text.RegularExpressions;
using IN.Chat.Web.Hubs.CommandProcessor.Core;
using IN.Chat.Web.Hubs.Entities;
using IN.Chat.Web.Hubs.Extensions;

namespace IN.Chat.Web.Hubs.CommandProcessor
{
    public class BaconCommandProcessor : ICommandProcessor
    {
        private static string[] URLS = new string[]
            {
                "http://bacolicious.s3.amazonaws.com/bacon.png",
                "http://www.jennyonthespot.com/wp-content/uploads/2010/05/Kevin-Bacon.jpg"
            };
        private static Regex REGEX = new Regex("(?i)^(/bacon )(me )?(.+)$");
        public string Name { get { return "Bacon"; } }
        public string Description { get { return "Returns an image of bacon, or an image of bacon on top of a website."; } }
        public string[] Usage { get { return new string[] { "/bacon", "/bacon <url>" }; } }

        public bool CanProcess(Message message)
        {
            return message.ProcessedContent.Equals("/bacon", StringComparison.OrdinalIgnoreCase) || REGEX.IsMatch(message.ProcessedContent);
        }

        public void Process(Message message)
        {
            if (CanProcess(message))
            {
                if (message.ProcessedContent.Equals("/bacon", StringComparison.OrdinalIgnoreCase))
                {
                    message.ProcessedContent = URLS.RandomElement();
                }
                else if (REGEX.IsMatch(message.ProcessedContent))
                {
                    var url = REGEX.Match(message.ProcessedContent).Groups[3].Value;
                    message.ProcessedContent = string.Format("http://bacolicio.us/{0}", url);
                }
            }
        }
    }
}