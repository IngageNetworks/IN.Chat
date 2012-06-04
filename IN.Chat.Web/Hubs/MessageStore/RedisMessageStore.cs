using System;
using System.Collections.Generic;
using System.Linq;
using BookSleeve;
using IN.Chat.Web.Hubs.Entities;
using IN.Chat.Web.Hubs.MessageStore.Core;
using Newtonsoft.Json;

namespace IN.Chat.Web.Hubs.MessageStore
{
    public class RedisMessageStore : IMessageStore
    {
        private const string REDISMESSAGESKEY = "IN:HackAThon:0001:Chat:Messages:All";
        private const int MAXMESSAGES = 50;
        private int Db { get; set; }
        private string Host { get; set; }
        private int Port { get; set; }
        private string Password { get; set; }

        public RedisMessageStore(int db, string redisUrl)
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

        public IEnumerable<Message> Read()
        {
            IEnumerable<Message> messages = new List<Message>();

            using (var connection = new RedisConnection(Host, Port, -1, Password))
            {
                connection.Open().Wait();
                if (connection.Keys.Exists(Db, REDISMESSAGESKEY).Result)
                {
                    var lastIndex = (int)(connection.Lists.GetLength(Db, REDISMESSAGESKEY).Result - 1);
                    messages = connection.Lists.RangeString(Db, REDISMESSAGESKEY, 0, lastIndex).Result.Select(m => JsonConvert.DeserializeObject<Message>(m));
                }
            }

            return messages;
        }

        public void Save(Message message)
        {
            var json = JsonConvert.SerializeObject(message);

            using (var connection = new RedisConnection(Host, Port, -1, Password))
            {
                connection.Open().Wait();
                connection.Lists.AddFirst(Db, REDISMESSAGESKEY, json).Wait();
                var listLength = connection.Lists.GetLength(Db, REDISMESSAGESKEY).Result;
                if (listLength > MAXMESSAGES)
                {
                    connection.Lists.Trim(Db, REDISMESSAGESKEY, MAXMESSAGES).Wait();
                }
            }
        }

        public void Clear()
        {
            using (var connection = new RedisConnection(Host, Port, -1, Password))
            {
                connection.Open().Wait();
                connection.Keys.Remove(Db, REDISMESSAGESKEY).Wait();
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