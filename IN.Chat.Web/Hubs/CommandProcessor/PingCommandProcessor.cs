using System;
using IN.Chat.Web.Hubs.CommandProcessor.Core;
using IN.Chat.Web.Hubs.Entities;

namespace IN.Chat.Web.Hubs.CommandProcessor
{
    public class PingCommandProcessor : ICommandProcessor
    {
        public string Name { get { return "Ping"; } }
        public string Description { get { return "Ping, Echo, and Time services"; } }
        public string[] Usage { get { return new string[] { "/ping", "/echo <text>", "/time" }; } }

        public bool CanProcess(Message message)
        {
            var canProcess = false;

            if (message.ProcessedContent.Equals("/ping", StringComparison.OrdinalIgnoreCase))
            {
                canProcess = true;
            }
            else if (message.ProcessedContent.StartsWith("/echo", StringComparison.OrdinalIgnoreCase))
            {
                canProcess = true;
            }
            else if (message.ProcessedContent.Equals("/time", StringComparison.OrdinalIgnoreCase))
            {
                canProcess = true;
            }

            return canProcess;
        }

        public void Process(Message message)
        {
            if (CanProcess(message))
            {
                if (message.ProcessedContent.Equals("/ping", StringComparison.OrdinalIgnoreCase))
                {
                    message.ProcessedContent = "pong";
                    message.From = ChatHubConfiguration.BotName;
                    message.Type = MessageType.Private;
                }
                else if (message.ProcessedContent.StartsWith("/echo ", StringComparison.OrdinalIgnoreCase))
                {
                    message.ProcessedContent = message.ProcessedContent.Substring(6);
                    message.From = ChatHubConfiguration.BotName;
                    message.Type = MessageType.Private;
                }
                else if (message.ProcessedContent.Equals("/time", StringComparison.OrdinalIgnoreCase))
                {
                    message.ProcessedContent = string.Format("{0:F} UTC", DateTime.UtcNow);
                    message.From = ChatHubConfiguration.BotName;
                    message.Type = MessageType.Private;
                }
            }
        }
    }
}