using System;
using System.Collections.Generic;
using BookSleeve;
using IN.Chat.Web.Hubs.Entities;
using IN.Chat.Web.Hubs.UserStore.Core;

namespace IN.Chat.Web.Hubs.UserStore
{
    public class RedisUserStore : IUserStore
    {
        private const string REDISUSERSKEYFORMAT = "IN:HackAThon:0001:Chat:Users:{0}";
        private int Db { get; set; }
        private string Host { get; set; }
        private int Port { get; set; }
        private string Password { get; set; }

        public RedisUserStore(int db, string redisUrl)
        {
            if (string.IsNullOrWhiteSpace(redisUrl))
            {
                throw new ArgumentNullException("redisUrl");
            }

            Db = db;
            Host = redisUrl.Split('@')[1].Split(':')[0];
            Port = int.Parse(redisUrl.Split('@')[1].Split(':')[1]);
            Password = redisUrl.Split('@')[0];
        }

        public IEnumerable<User> Read()
        {
            var users = new List<User>();

            var pattern = string.Format(REDISUSERSKEYFORMAT, "*");
            using (var connection = new RedisConnection(Host, Port, -1, Password))
            {
                connection.Open().Wait();
                var keys = connection.Keys.Find(Db, pattern).Result; //Yea, I know this is bad.
                foreach (var key in keys)
                {
                    var value = connection.Strings.GetString(Db, key).Result;
                    //var ttl = connection.Keys.TimeToLive(Db, key).Result;
                    users.Add(new User
                    {
                        ConnectionId = key,
                        Username = value,
                    });
                }
            }

            return users;
        }

        public void Save(string username, string connectionId)
        {
            var key = string.Format(REDISUSERSKEYFORMAT, connectionId);
            var value = username.ToLower();
            var expireIn = (long)(new TimeSpan(0, 2, 0).TotalSeconds);

            using (var connection = new RedisConnection(Host, Port, -1, Password))
            {
                connection.Open().Wait();
                connection.Strings.Set(Db, key, value, expireIn).Wait();
            }
        }

        public void Delete(string connectionId)
        {
            var key = string.Format(REDISUSERSKEYFORMAT, connectionId);
            using (var connection = new RedisConnection(Host, Port, -1, Password))
            {
                connection.Open().Wait();
                connection.Keys.Remove(Db, key).Wait();
            }
        }

        public void Clear()
        {
            var pattern = string.Format(REDISUSERSKEYFORMAT, "*");
            using (var connection = new RedisConnection(Host, Port, -1, Password))
            {
                connection.Open().Wait();
                var keys = connection.Keys.Find(Db, pattern).Result; //Yea, I know this is bad.
                foreach (var key in keys)
                {
                    connection.Keys.Remove(Db, key).Wait();
                }
            }
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