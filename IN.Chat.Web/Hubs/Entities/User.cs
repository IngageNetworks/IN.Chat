using System.Collections.Generic;

namespace IN.Chat.Web.Hubs.Entities
{
    public class User
    {
        public string Username { get; set; }
        public string ConnectionId { get; set; }
    }

    public class UserEqualityComparer : IEqualityComparer<User>
    {
        public bool Equals(User x, User y)
        {
            return x.Username.Equals(y.Username);
        }

        public int GetHashCode(User obj)
        {
            return obj.GetHashCode();
        }
    }
}