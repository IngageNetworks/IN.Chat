using System;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Mvc;

namespace IN.Chat.Web.Controllers
{
    public class BaseController : Controller
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected static string INAPIKEY = ConfigurationManager.AppSettings["in_apikey"];
        protected static string INADMINUSERNAME = ConfigurationManager.AppSettings["in_admin_username"];
        protected static string INADMINPASSWORD = ConfigurationManager.AppSettings["in_admin_password"];
        protected static string INAUTHURL = ConfigurationManager.AppSettings["in_auth_url"];
        protected static string INUSERSCREATEURL = ConfigurationManager.AppSettings["in_users_create_url"];
        protected static string INUSERSSECURITYQUESTIONSURL = ConfigurationManager.AppSettings["in_users_securityquestions_url"];
        protected static string INUSERSMEURL = ConfigurationManager.AppSettings["in_users_me_url"];

        public BaseController()
        {
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            var exception = filterContext.Exception;
            var baseException = exception.GetBaseException();
            Logger.ErrorException("An unhandled controller exception occured.", exception);
        }

        protected static AuthenticationHeaderValue CreateBasicAuthorizationHeader(string username, string password)
        {
            var plainTextCredentials = string.Concat(username, ":", password);
            var byteCredentials = ASCIIEncoding.ASCII.GetBytes(plainTextCredentials);
            var base64Credentials = Convert.ToBase64String(byteCredentials);
            return new AuthenticationHeaderValue("Basic", base64Credentials);
        }
    }
}
