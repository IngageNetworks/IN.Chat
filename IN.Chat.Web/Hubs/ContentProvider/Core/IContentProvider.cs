using System.ComponentModel.Composition;
using IN.Chat.Web.Hubs.Entities;

namespace IN.Chat.Web.Hubs.ContentProvider.Core
{
    [InheritedExport]
    public interface IContentProvider
    {
        string Name { get; }
        string Description { get; }

        bool ContainsContent(Message message);

        void ExtractContent(Message message);
    }
}