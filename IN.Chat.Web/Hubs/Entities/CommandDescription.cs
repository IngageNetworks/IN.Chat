using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IN.Chat.Web.Hubs.Entities
{
    public class CommandDescription
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Usage { get; set; }
    }
}