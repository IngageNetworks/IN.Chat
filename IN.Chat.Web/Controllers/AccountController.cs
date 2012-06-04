using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Mvc;
using IN.Chat.Web.Models.Account;
using Newtonsoft.Json;

namespace IN.Chat.Web.Controllers
{
    public class AccountController : BaseController
    {
        private static string INAPIKEY = ConfigurationManager.AppSettings["in_apikey"];
        private static string INACCESSTOKEN = ConfigurationManager.AppSettings["in_accesstoken"];
        private static string INUSERSCREATESURL = ConfigurationManager.AppSettings["in_users_create_url"];

        [HttpGet]
        public ActionResult Create()
        {
            return View(new CreateModel());
        }

        [HttpPost]
        public ActionResult Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                string errorMessage;
                if (TryCreateAccount(model, out errorMessage))
                {
                    //todo: Email user's credentials
                    return View("CreateSuccess");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errorMessage);
                    return Create();
                }
            }
            else
            {
                return Create();
            }
        }

        private static bool TryCreateAccount(CreateModel model, out string errorMessage)
        {
            var success = false;
            errorMessage = string.Empty;

            try
            {
                if (!string.IsNullOrWhiteSpace(INUSERSCREATESURL) && !string.IsNullOrWhiteSpace(INAPIKEY))
                {
                    var requestBody = new
                    {
                        Authority = new
                        {
                            IsTenantAdmin = false,
                        },
                        Credentials = new
                        {
                            Password = model.Password,
                            Username = model.Username,
                        },
                        Profile = new
                        {
                            EmailAddress = model.EmailAddress,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                        }
                    };

                    ServicePointManager.UseNagleAlgorithm = false;
                    ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("api-key", INAPIKEY);
                    httpClient.DefaultRequestHeaders.Add("x-sts-accesstoken", INACCESSTOKEN);
                    var httpContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    var result = httpClient.PostAsync(INUSERSCREATESURL, httpContent).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        success = true;
                    }
                    else
                    {
                        errorMessage = result.ReasonPhrase;
                        var content = result.Content.ReadAsStringAsync().Result;
                        dynamic responseJson = JsonConvert.DeserializeObject(content);
                        errorMessage = responseJson.Message.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                var baseException = ex.GetBaseException();
                errorMessage = "Sorry. An unexpeted error occured, while creating your account.";
            }

            return success;
        }
    }
}
