namespace IN.Chat.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            Logger.Info("Application starting...");
            Bootstrapper.Bootstrapper.PerApplication();
            Logger.Info("Application started");
        }

        public override void Init()
        {
            base.Init();
            Bootstrapper.Bootstrapper.PerInstance(this);
        }
    }
}