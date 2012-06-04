using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace IN.Chat.Web.Controllers.Attributes
{
    public class STSAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var encryptedCookie = httpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (encryptedCookie != null)
            {
                var decryptedCookie = FormsAuthentication.Decrypt(encryptedCookie.Value);
                if (!decryptedCookie.Expired)
                {
                    FormsAuthentication.RenewTicketIfOld(decryptedCookie);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult("Login", null);
        }
    }
}