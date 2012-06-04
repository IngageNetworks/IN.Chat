using System.Web;

namespace IN.Chat.Web.Bootstrapper.Tasks.Core
{
    internal interface IBootstrapperPerInstanceTask
    {
        void Execute(HttpApplication application);
    }
}