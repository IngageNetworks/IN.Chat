using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using IN.Chat.Web.Models.Credentials;
using Newtonsoft.Json;

namespace IN.Chat.Web.Controllers
{
    public class LoginController : BaseController
    {
        private static string INAPIKEY = ConfigurationManager.AppSettings["in_apikey"];
        private static string INAUTHURL = ConfigurationManager.AppSettings["in_auth_url"];
        private static string INUSERSMESURL = ConfigurationManager.AppSettings["in_users_me_url"];

        [HttpGet]
        public ActionResult Login()
        {
            return View(new CredentialsModel());
        }

        [HttpPost]
        public ActionResult Login(CredentialsModel model)
        {
            if (ModelState.IsValid)
            {
                string accessToken;
                string username;
                if (TryGetAccessToken(model, out accessToken) && TryGetUserName(accessToken, out username))
                {
                    var ticket = new FormsAuthenticationTicket(1, username, DateTime.UtcNow, DateTime.MaxValue, true, accessToken);
                    var encryptedTicket = FormsAuthentication.Encrypt(ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    Response.AppendCookie(cookie);
                    return RedirectToRoute("Chat");
                }
                else
                {
                    ModelState.AddModelError("Username", "Invalid Credentials");
                    return Login();
                }
            }
            else
            {
                return Login();
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToRoute("Login");
        }

        private bool TryGetAccessToken(CredentialsModel model, out string accessToken)
        {
            var success = false;
            accessToken = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(INAUTHURL) && !string.IsNullOrWhiteSpace(INAPIKEY))
                {
                    ServicePointManager.UseNagleAlgorithm = false;
                    ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(model.Username + ":" + model.Password)));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("api-key", INAPIKEY);
                    var result = httpClient.GetAsync(INAUTHURL).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        var content = result.Content.ReadAsStringAsync().Result;
                        dynamic json = JsonConvert.DeserializeObject(content);
                        if (!string.IsNullOrWhiteSpace(json.AccessToken.Value))
                        {
                            accessToken = json.AccessToken;
                            success = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var baseException = ex.GetBaseException();
            }

            return success;
        }

        private bool TryGetUserName(string accessToken, out string username)
        {
            var success = false;
            username = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(INUSERSMESURL))
                {
                    ServicePointManager.UseNagleAlgorithm = false;
                    ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("api-key", INAPIKEY);
                    httpClient.DefaultRequestHeaders.Add("x-sts-accesstoken", accessToken);
                    var result = httpClient.GetAsync(INUSERSMESURL).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        var content = result.Content.ReadAsStringAsync().Result;
                        dynamic json = JsonConvert.DeserializeObject(content);
                        if (!string.IsNullOrWhiteSpace(json.Username.Value))
                        {
                            username = json.Username;
                            success = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var baseException = ex.GetBaseException();
            }

            return success;
        }
    }
}
