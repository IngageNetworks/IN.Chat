using System.Web.Optimization;
using IN.Chat.Web.Bootstrapper.Tasks.Core;

namespace IN.Chat.Web.Bootstrapper.Tasks
{
    public class Bundles : IBootstrapperPerApplicationTask
    {
        public void Execute()
        {
            BundleTable.Bundles.EnableDefaultBundles();
        }
    }
}