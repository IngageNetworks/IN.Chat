using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Mvc;
using IN.Chat.Web.Models.Account;
using Newtonsoft.Json;

namespace IN.Chat.Web.Controllers
{
    public class AccountController : BaseController
    {
        [HttpGet]
        public ActionResult Create()
        {
            var accessToken = GetAdminAccessToken(INAUTHURL, INAPIKEY, INADMINUSERNAME, INADMINPASSWORD);
            var securityQuestions = GetSecurityQuestions(INUSERSSECURITYQUESTIONSURL, INAPIKEY, accessToken);
            var model = new CreateModel()
            {
                SecurityQuestions = securityQuestions
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                var accessToken = GetAdminAccessToken(INAUTHURL, INAPIKEY, INADMINUSERNAME, INADMINPASSWORD);

                string errorMessage;
                if (TryCreateAccount(INUSERSCREATEURL, INAPIKEY, accessToken, model, out errorMessage))
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

        private static IEnumerable<SelectListItem> GetSecurityQuestions(string securityQuestionsUrl, string apiKey, string accessToken)
        {
            var securityQuestions = new List<SelectListItem>();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
                httpClient.DefaultRequestHeaders.Add("x-sts-accesstoken", accessToken);
                var result = httpClient.GetAsync(securityQuestionsUrl).Result.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject(result) as dynamic;
                var questions = json.Entities as IEnumerable<dynamic>;

                foreach (var question in questions)
                {
                    securityQuestions.Add(new SelectListItem()
                    {
                        Selected = false,
                        Text = question.Question.Value.ToString(),
                        Value = question.Id.Value.ToString()
                    });
                }
            }

            return securityQuestions;
        }

        private static string GetAdminAccessToken(string authUrl, string apiKey, string username, string password)
        {
            var accessToken = string.Empty;

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = CreateBasicAuthorizationHeader(username, password);
                httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
                var result = httpClient.GetAsync(authUrl).Result;

                if (result.IsSuccessStatusCode)
                {
                    var resultBody = result.Content.ReadAsStringAsync().Result;
                    var json = JsonConvert.DeserializeObject(resultBody) as dynamic;
                    accessToken = json.AccessToken.Value.ToString();
                }
            }

            return accessToken;
        }

        private static bool TryCreateAccount(string createUserUrl, string apiKey, string accessToken, CreateModel model, out string errorMessage)
        {
            var success = false;
            errorMessage = string.Empty;

            try
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
                        Pin = model.Pin,
                        SecurityQuestionAnswers = new[] 
                        {
                            new 
                            {
                                Answer = model.QuestionOneAnswer,
                                SecurityQuestionId = model.QuestionOneId,
                            },
                            new 
                            {
                                Answer = model.QuestionTwoAnswer,
                                SecurityQuestionId = model.QuestionTwoId,
                            },
                            new 
                            {
                                Answer = model.QuestionThreeAnswer,
                                SecurityQuestionId = model.QuestionThreeId,
                            },
                        },
                        Username = model.Username,
                    },
                    Profile = new
                    {
                        EmailAddress = model.EmailAddress,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                    }
                };

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
                    httpClient.DefaultRequestHeaders.Add("x-sts-accesstoken", accessToken);
                    var httpContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    var result = httpClient.PostAsync(createUserUrl, httpContent).Result;

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
