using System;
using System.Collections.Generic;
using IN.Chat.Web.Hubs.Entities;

namespace IN.Chat.Web.Hubs.MessageStore.Core
{
    public interface IMessageStore : IDisposable
    {
        IEnumerable<Message> Read();

        void Save(Message message);

        void Clear();
    }
}