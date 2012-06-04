using System;
using System.Threading.Tasks;
using System.Web;
using IN.Chat.Web.Bootstrapper.Tasks.Core;

namespace IN.Chat.Web.Bootstrapper.Tasks
{
    public class RegisterErrorHandling : IBootstrapperPerInstanceTask
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public void Execute(HttpApplication context)
        {
            TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>(TaskScheduler_UnobservedTaskException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            context.Error += new EventHandler(context_Error);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            var baseException = exception.GetBaseException();

            if (e.IsTerminating)
            {
                Logger.ErrorException("An unhandled domain exception occured, causing the application to terminate.", exception);
            }
            else
            {
                Logger.ErrorException("An unhandled domain exception occured.", exception);
            }
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var exception = e.Exception;
            var baseException = e.Exception.GetBaseException();
            Logger.ErrorException("An unobserved task exception occured.", exception);
            e.SetObserved();
        }

        void context_Error(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                var context = sender as HttpApplication;
                var exception = context.Server.GetLastError().GetBaseException();
                var code = (exception is HttpException) ? (exception as HttpException).GetHttpCode() : 500;

                if (!code.Equals(404))
                {
                    var message = exception.Message;
                    Logger.ErrorException("An unhandled http application exception occured.", exception);
                }

                context.Server.ClearError();
            }
        }
    }
}