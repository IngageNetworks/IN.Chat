using System;
using System.Collections.Generic;
using IN.Chat.Web.Hubs.Entities;
using IN.Chat.Web.Hubs.UserStore.Core;

namespace IN.Chat.Web.Hubs.UserStore
{
    public class InMemoryUserStore : IUserStore
    {
        private List<User> Users { get; set; }

        public InMemoryUserStore()
        {
            Users = new List<User>();
        }

        public IEnumerable<User> Read()
        {
            return Users;
        }

        public void Save(string username, string connectionId)
        {
            if (!Users.Exists(u => u.ConnectionId.Equals(connectionId)))
            {
                Users.Add(new User { Username = username.ToLower(), ConnectionId = connectionId });
            }
        }

        public void Delete(string connectionId)
        {
            Users.RemoveAll(u => u.ConnectionId.Equals(connectionId));
        }

        public void Clear()
        {
            Users.Clear();
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