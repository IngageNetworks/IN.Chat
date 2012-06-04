using System.Web.Mvc;
using IN.Chat.Web.Controllers.Attributes;

namespace IN.Chat.Web.Controllers
{
    public class BaseController : Controller
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected override void OnException(ExceptionContext filterContext)
        {
            var exception = filterContext.Exception;
            var baseException = exception.GetBaseException();
            Logger.ErrorException("An unhandled controller exception occured.", exception);
        }
    }
}
