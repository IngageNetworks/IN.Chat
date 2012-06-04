using System.Web.Mvc;
using IN.Chat.Web.Controllers.Attributes;

namespace IN.Chat.Web.Controllers
{
    public class HomeController : BaseController
    {
        [STSAuthorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}
