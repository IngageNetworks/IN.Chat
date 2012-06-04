using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Web;
using IN.Chat.Web.Bootstrapper.Tasks;
using IN.Chat.Web.Bootstrapper.Tasks.Core;
using IN.Chat.Web.Hubs;
using IN.Chat.Web.Hubs.CommandProcessor.Core;
using IN.Chat.Web.Hubs.ContentProvider.Core;
using IN.Chat.Web.Hubs.MessageStore;
using IN.Chat.Web.Hubs.MessageStore.Core;
using IN.Chat.Web.Hubs.UserStore;
using IN.Chat.Web.Hubs.UserStore.Core;
using TinyIoC;

namespace IN.Chat.Web.Bootstrapper
{
    internal static class Bootstrapper
    {
        private static Lazy<TinyIoCContainer> Container = new Lazy<TinyIoCContainer>(() => GetIoCContainer());
        private static Lazy<IEnumerable<IBootstrapperPerApplicationTask>> PerApplicationTasks = new Lazy<IEnumerable<IBootstrapperPerApplicationTask>>(() => Container.Value.ResolveAll<IBootstrapperPerApplicationTask>(true));
        private static Lazy<IEnumerable<IBootstrapperPerInstanceTask>> PerInstanceTasks = new Lazy<IEnumerable<IBootstrapperPerInstanceTask>>(() => Container.Value.ResolveAll<IBootstrapperPerInstanceTask>(true));

        public static void PerInstance(HttpApplication httpApplication)
        {
            foreach (var task in PerInstanceTasks.Value)
            {
                task.Execute(httpApplication);
            }
        }

        public static void PerApplication()
        {
            foreach (var task in PerApplicationTasks.Value)
            {
                task.Execute();
            }
        }

        private static TinyIoCContainer GetIoCContainer()
        {
            var container = new TinyIoCContainer();

            //Configuration Values
            var redisToGoUrl = ConfigurationManager.AppSettings["REDISTOGO_URL"];
            var botName = ConfigurationManager.AppSettings["BotName"];

            //Using MEF to build-up instances
            var commandProcessorCompositionContainer = new CompositionContainer(new AssemblyCatalog(typeof(ICommandProcessor).Assembly));
            var commandProcessors = commandProcessorCompositionContainer.GetExportedValues<ICommandProcessor>();
            var contentProviderCompositionContainer = new CompositionContainer(new AssemblyCatalog(typeof(IContentProvider).Assembly));
            var contentProviders = contentProviderCompositionContainer.GetExportedValues<IContentProvider>();

            //Bootstrapper Tasks
            container.Register<IBootstrapperPerInstanceTask, RegisterErrorHandling>("RegisterErrorHandling");
            container.Register<IBootstrapperPerApplicationTask, Bundles>("Bundles");
            container.Register<IBootstrapperPerApplicationTask, RegisterRoutes>("RegisterRoutes");
            container.Register<Func<ChatHub>>((c, p) => () => c.Resolve<ChatHub>());
            container.Register<IBootstrapperPerApplicationTask, RegisterSignalRDependencies>("RegisterSignalRDependencies");
            //Domain
            container.Register<string>((c, p) => botName);
            container.Register<IEnumerable<ICommandProcessor>>(commandProcessors); //Singleton
            container.Register<IEnumerable<IContentProvider>>(contentProviders); //Singleton
            container.Register<IMessageStore>((c, p) => new RedisMessageStore(0, redisToGoUrl)); //Multi-instance
            container.Register<IUserStore>((c, p) => new RedisUserStore(0, redisToGoUrl)); //Multi-instance
            container.Register<ChatHub>(); //Multi-instance

            return container;
        }
    }
}