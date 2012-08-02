using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using IN.Chat.Web.Models.Credentials;
using Newtonsoft.Json;

namespace IN.Chat.Web.Controllers
{
    public class LoginController : BaseController
    {
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
                if (TryGetAccessToken(INAUTHURL, INAPIKEY, model, out accessToken) && TryGetUserName(INUSERSMEURL, INAPIKEY, accessToken, out username))
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

        private static bool TryGetAccessToken(string authUrl, string apiKey, CredentialsModel model, out string accessToken)
        {
            var success = false;
            accessToken = null;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = CreateBasicAuthorizationHeader(model.Username, model.Password);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
                    var result = httpClient.GetAsync(authUrl).Result;

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

        private static bool TryGetUserName(string meUrl, string apiKey, string accessToken, out string username)
        {
            var success = false;
            username = null;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
                    httpClient.DefaultRequestHeaders.Add("x-sts-accesstoken", accessToken);
                    var result = httpClient.GetAsync(meUrl).Result;

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
