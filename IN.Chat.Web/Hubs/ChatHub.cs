using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using IN.Chat.Web.Hubs.CommandProcessor.Core;
using IN.Chat.Web.Hubs.ContentProvider.Core;
using IN.Chat.Web.Hubs.Entities;
using IN.Chat.Web.Hubs.MessageStore.Core;
using IN.Chat.Web.Hubs.Services;
using IN.Chat.Web.Hubs.UserStore.Core;
using SignalR.Hubs;

namespace IN.Chat.Web.Hubs
{
    [HubName("chat")]
    public class ChatHub : Hub, IDisconnect, IConnected
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static IEnumerable<ICommandProcessor> CommandProcessors { get; set; }
        private static IEnumerable<CommandDescription> CommandProcessorDescriptions { get; set; }
        private static IEnumerable<IContentProvider> ContentProviders { get; set; }
        private static IEnumerable<ContentProviderDescription> ContentProviderDescriptions { get; set; }
        private IMessageStore MessageStore { get; set; }
        private IUserStore UserStore { get; set; }

        public ChatHub(string botName, IMessageStore messageStore, IUserStore userStore, IEnumerable<ICommandProcessor> commandProcessors, IEnumerable<IContentProvider> contentProviders)
        {
            if (string.IsNullOrWhiteSpace(botName))
            {
                throw new ArgumentNullException("botName");
            }

            if (messageStore == null)
            {
                throw new ArgumentNullException("messageStore");
            }

            if (userStore == null)
            {
                throw new ArgumentNullException("userStore");
            }

            if (commandProcessors == null)
            {
                throw new ArgumentNullException("commandProcessors");
            }

            if (contentProviders == null)
            {
                throw new ArgumentNullException("contentProviders");
            }

            ChatHubConfiguration.BotName = botName;
            MessageStore = messageStore;
            UserStore = userStore;
            CommandProcessors = commandProcessors;
            CommandProcessorDescriptions = CommandProcessors.Select(p => new CommandDescription() { Description = p.Description, Name = p.Name, Usage = p.Usage }).OrderBy(d => d.Name) as IEnumerable<CommandDescription>;
            ContentProviders = contentProviders;
            ContentProviderDescriptions = ContentProviders.Select(p => new ContentProviderDescription() { Description = p.Description, Name = p.Name, }).OrderBy(d => d.Name) as IEnumerable<ContentProviderDescription>;
        }

        public IEnumerable<Message> GetRecentMessages()
        {
            IEnumerable<Message> messages = new List<Message>();
            try
            {
                if (Context.User.Identity.IsAuthenticated)
                {
                    messages = MessageStore.Read().OrderBy(m => m.Timestamp);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An exception occured while reading messages.", ex);
            }
            return messages;
        }

        public IEnumerable<string> GetConnectedUsers()
        {
            IEnumerable<string> users = new List<string>();
            try
            {
                if (Context.User.Identity.IsAuthenticated)
                {
                    users = UserStore.Read().Select(u => u.Username).Distinct();
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An exception occured while reading users.", ex);
            }
            return users;
        }

        public IEnumerable<ContentProviderDescription> GetContentProviderDescriptions()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                return ContentProviderDescriptions;
            }
            else
            {
                return new List<ContentProviderDescription>();
            }
        }

        public IEnumerable<CommandDescription> GetCommandDescriptions()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                return CommandProcessorDescriptions;
            }
            else
            {
                return new List<CommandDescription>();
            }
        }

        public void Send(string content)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                var message = ParseMessage(content);
                Send(message);
            }
        }

        private void Send(Message message)
        {
            ProcessMessage(message);
            StoreMessage(message);
            SendMessage(message);
        }

        public Task Disconnect()
        {
            try
            {
                UserStore.Delete(Context.ConnectionId);
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An exception occured while deleting a user.", ex);
            }
            
            return Clients.leave();
        }

        public Task Connect()
        {
            try
            {
                if (Context.User.Identity.IsAuthenticated)
                {
                    UserStore.Save(Context.User.Identity.Name, Context.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An exception occured while saving a user.", ex);
            }

            return Clients.joined();
        }

        public Task Reconnect(IEnumerable<string> groups)
        {
            try
            {
                if (Context.User.Identity.IsAuthenticated)
                {
                    UserStore.Save(Context.User.Identity.Name, Context.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An exception occured while saving a user.", ex);
            }
            
            return Clients.rejoined();
        }

        private Message ParseMessage(string message)
        {
            var privateMessageRegex = new Regex("^(@)(.+?)( )(.+)");

            var parsedMessage = new Message
            {
                Timestamp = DateTime.UtcNow,
                From = Context.User.Identity.Name,
                RawContent = message
            };

            try
            {
                if (privateMessageRegex.IsMatch(message))
                {
                    var match = privateMessageRegex.Match(message);
                    var username = match.Groups[2].Value;
                    var privateMessage = match.Groups[4].Value;

                    parsedMessage.ProcessedContent = privateMessage;
                    parsedMessage.Type = MessageType.Private;
                    parsedMessage.To = username;
                }
                else
                {
                    parsedMessage.ProcessedContent = message;
                    parsedMessage.Type = MessageType.Public;
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An exception occured while parsing a message.", ex);
            }

            return parsedMessage;
        }

        private void StoreMessage(Message message)
        {
            try
            {
                if (message.Type.Equals(MessageType.Public))
                {
                    MessageStore.Save(message);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An exception occured while saving a message.", ex);
            }
        }

        private Message ProcessMessage(Message message)
        {
            try
            {
                message.ProcessedContent = message.ProcessedContent.Trim();
                message.ProcessedContent = HttpUtility.HtmlEncode(message.ProcessedContent);

                foreach (var commandProcessor in CommandProcessors.Where(p => p.CanProcess(message)))
                {
                    commandProcessor.Process(message);
                }

                message.ProcessedContent = TextTransformer.TransformUrls(message.ProcessedContent);
                message.ProcessedContent = TextTransformer.FormatMultiLineContent(message.ProcessedContent);

                foreach (var contentProvider in ContentProviders.Where(p => p.ContainsContent(message)))
                {
                    contentProvider.ExtractContent(message);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An exception occured while processing a message.", ex);
            }

            return message;
        }

        private void SendMessage(Message message)
        {
            try
            {
                switch (message.Type)
                {
                    case MessageType.Public:
                        SendPublicMessage(message);
                        break;
                    case MessageType.Private:
                        SendPrivateMessage(message);
                        break;
                    case MessageType.Unknown:
                    default:
                        throw new ArgumentException("Unknown MessageType");
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An exception occured while sending a message.", ex);
            }
        }

        private void SendPublicMessage(Message message)
        {
            try
            {
                Clients.addMessage(message);
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An exception occured while sending a public message.", ex);
            }
        }

        private void SendPrivateMessage(Message message)
        {
            try
            {
                Clients[Context.ConnectionId].addMessage(message);

                if (!string.IsNullOrWhiteSpace(message.To))
                {
                    var users = UserStore.Read().Where(u => u.Username.Equals(message.To.ToLower()));
                    if (users != null && users.Any())
                    {
                        foreach (var user in users)
                        {
                            Clients[user.ConnectionId].addMessage(message);
                        }
                    }
                    else
                    {
                        message.From = ChatHubConfiguration.BotName;
                        message.ProcessedContent = string.Format("Unknown username: @{0}", message.To);
                        Clients[Context.ConnectionId].addMessage(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An exception occured while sending a private message.", ex);
            }
        }
    }
}