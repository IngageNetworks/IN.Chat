using IN.Chat.Web.Hubs.Entities;

namespace IN.Chat.Web.Hubs.ContentProvider.Core
{
    public abstract class EmbeddedContentProvider : IContentProvider
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        protected string EmbeddedContentMarkUpFormat 
        { 
            get 
            {
                return "<div class=\"embedded-content alert alert-default\"><blockquote><small>{0}</small></blockquote>{1}</div>"; 
            } 
        }
        
        public abstract bool ContainsContent(Message message);

        public abstract void ExtractContent(Message message);
    }
}