using System;
using System.Collections.Generic;
using IN.Chat.Web.Hubs.Entities;
using IN.Chat.Web.Hubs.MessageStore.Core;

namespace IN.Chat.Web.Hubs.MessageStore
{
    public class InMemoryMessageStore : IMessageStore
    {
        private List<Message> Messages { get; set; }

        private int MaxMessages { get { return 150; } }

        public InMemoryMessageStore()
        {
            Messages = new List<Message>();
        }

        public IEnumerable<Message> Read()
        {
            return Messages;
        }

        public void Save(Message message)
        {
            Messages.Add(message);
        }

        public void Clear()
        {
            Messages.Clear();
        }

        #region IDisposable Member(s)

        private bool Disposed { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.Disposed)
            {
                if (disposing)
                {
                }
                this.Disposed = true;
            }
        }

        #endregion IDisposable Member(s)
    }
}