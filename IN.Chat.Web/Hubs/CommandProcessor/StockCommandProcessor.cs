using System.Text.RegularExpressions;
using IN.Chat.Web.Hubs.CommandProcessor.Core;
using IN.Chat.Web.Hubs.Entities;

namespace IN.Chat.Web.Hubs.CommandProcessor
{
    public class StockCommandProcessor : ICommandProcessor
    {
        private const string URLFORMAT = "http://chart.finance.yahoo.com/z?s={0}&t={1}&q=l&l=on&z=l&a=v&p=s&lang=en-US&region=US#.png";
        private static Regex REGEX = new Regex(@"(?i)^(/stock )(me |for )?(.+)$");
        public string Name { get { return "Stock"; } }
        public string Description { get { return "Stock information"; } }
        public string[] Usage { get { return new string[] { "/stock <ticker>" }; } }

        public bool CanProcess(Message message)
        {
            return REGEX.IsMatch(message.ProcessedContent);
        }

        public void Process(Message message)
        {
            if (CanProcess(message))
            {
                var symbol = REGEX.Match(message.ProcessedContent).Groups[3].Value;
                message.ProcessedContent = GetStockInformation(symbol);
            }
        }

        private string GetStockInformation(string symbol)
        {
            return string.Format(URLFORMAT, symbol, "5d");
        }
    }
}