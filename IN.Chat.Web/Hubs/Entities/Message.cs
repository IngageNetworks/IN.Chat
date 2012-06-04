using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IN.Chat.Web.Hubs.Entities
{
    public enum MessageType : int
    {
        Unknown = 0,
        Public = 1,
        Private = 2,
    }

    public class Message
    {
        public DateTime Timestamp { get; set; }
        internal string To { get; set; }
        public string From { get; set; }
        public string RawContent { get; set; }
        public string ProcessedContent { get; set; }
        public MessageType Type { get; set; }
    }
}