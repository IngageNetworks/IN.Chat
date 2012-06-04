using System.Web.Mvc;

namespace IN.Chat.Web.Controllers
{
    public class ErrorController : BaseController
    {
        public ActionResult Notfound()
        {
            return RedirectToRoute("Chat");
        }
    }
}
