using System;
using System.Collections.Generic;
using IN.Chat.Web.Hubs.Entities;

namespace IN.Chat.Web.Hubs.UserStore.Core
{
    public interface IUserStore : IDisposable
    {
        IEnumerable<User> Read();

        void Save(string username, string connectionId);

        void Delete(string connectionId);

        void Clear();
    }
}