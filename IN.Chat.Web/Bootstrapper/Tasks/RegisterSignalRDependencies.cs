using System;
using IN.Chat.Web.Bootstrapper.Tasks.Core;
using IN.Chat.Web.Hubs;
using SignalR;

namespace IN.Chat.Web.Bootstrapper.Tasks
{
    public class RegisterSignalRDependencies : IBootstrapperPerApplicationTask
    {
        private Func<ChatHub> ChatHubFactory { get; set; }

        public RegisterSignalRDependencies(Func<ChatHub> chatHubFactory)
        {
            if (chatHubFactory == null)
            {
                throw new ArgumentNullException("chatHubFactory");
            }

            ChatHubFactory = chatHubFactory;
        }

        public void Execute()
        {
            GlobalHost.DependencyResolver.Register(typeof(ChatHub), ChatHubFactory);
        }
    }
}