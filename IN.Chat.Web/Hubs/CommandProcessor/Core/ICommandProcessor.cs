using System.ComponentModel.Composition;
using IN.Chat.Web.Hubs.Entities;

namespace IN.Chat.Web.Hubs.CommandProcessor.Core
{
    [InheritedExport]
    public interface ICommandProcessor
    {
        string Name { get; }
        string Description { get; }
        string[] Usage { get; }

        bool CanProcess(Message message);

        void Process(Message message);
    }
}