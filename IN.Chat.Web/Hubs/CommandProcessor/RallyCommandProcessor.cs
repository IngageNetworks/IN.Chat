using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using IN.Chat.Web.Hubs.CommandProcessor.Core;
using IN.Chat.Web.Hubs.Entities;
using Newtonsoft.Json;

namespace IN.Chat.Web.Hubs.CommandProcessor
{
    public class RallyCommandProcessor : ICommandProcessor
    {
        private const string ApiUrl = "https://rally1.rallydev.com/slm/webservice/1.29/";
        private static string Username = ConfigurationManager.AppSettings["Rally_Username"];
        private static string Password = ConfigurationManager.AppSettings["Rally_Password"];
        private static Regex[] REGEXES = new Regex[] 
        { 
            new Regex("(?i)^(/rally )(me )?(us)(.+)$"),
            new Regex("(?i)^(/rally )(me )?(ta)(.+)$"),
            new Regex("(?i)^(/rally )(me )?(de)(.+)$"),
        };
        public string Name { get { return "Rally"; } }
        public string Description { get { return "Returns the specified Rally item."; } }
        public string[] Usage { get { return new string[] { "/rally us<number>", "/rally ta<number>", "/rally de<number>" }; } }

        public bool CanProcess(Message message)
        {
            return REGEXES.Any(r => r.IsMatch(message.ProcessedContent));
        }

        public void Process(Message message)
        {
            if (CanProcess(message))
            {
                var regex = REGEXES.First(r => r.IsMatch(message.RawContent));
                var type = regex.Match(message.RawContent).Groups[3].Value;
                var number = regex.Match(message.RawContent).Groups[4].Value;

                if (type.Equals("de", StringComparison.OrdinalIgnoreCase))
                {
                    message.ProcessedContent = GetDefect(number);
                }
                else if(type.Equals("ta", StringComparison.OrdinalIgnoreCase))
                {
                    message.ProcessedContent = GetTask(number);
                }
                else if (type.Equals("us", StringComparison.OrdinalIgnoreCase))
                {
                    message.ProcessedContent = GetStory(number);
                }
            }
        }

        private static dynamic GetItem(string url)
        {
            dynamic item = null;

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-us"));
            httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(Username + ":" + Password)));
            var result = httpClient.GetAsync(url).Result.Content.ReadAsStringAsync().Result;

            item = JsonConvert.DeserializeObject(result);

            return item;
        }

        private static string GetDefect(string number)
        {
            var builder = new StringBuilder();
            var url = string.Format("{0}defect.js?query=(FormattedId%20=%20de{1})&fetch=true", ApiUrl, number);
            var item = GetItem(url);

            if (item != null && item.QueryResult.TotalResultCount > 0)
            {
                int deisredLabelLength = 8;
                builder.AppendLine(string.Format("{0}", item.QueryResult.Results[0].FormattedID));
                builder.AppendLine("===");
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Title", deisredLabelLength), item.QueryResult.Results[0].Name));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Owner", deisredLabelLength), item.QueryResult.Results[0].Owner == null ? "No Owner" : item.QueryResult.Results[0].Owner._refObjectName));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Project", deisredLabelLength), item.QueryResult.Results[0].Project == null ? "No Project" : item.QueryResult.Results[0].Project._refObjectName));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Severity", deisredLabelLength), item.QueryResult.Results[0].Severity));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("State", deisredLabelLength), item.QueryResult.Results[0].State));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Link", deisredLabelLength), GetLinkToItem(item.QueryResult.Results[0], "defect")));
                builder.AppendLine();
                builder.AppendLine(FormatDescription(item.QueryResult.Results[0].Description.Value));
            }
            else
            {
                builder.Append(string.Format("Sorry, I could'nt find de{0}", number));
            }

            return builder.ToString();
        }

        private static string GetTask(string number)
        {
            var builder = new StringBuilder();
            var url = string.Format("{0}task.js?query=(FormattedId%20=%20de{1})&fetch=true", ApiUrl, number);
            var item = GetItem(url);

            if (item != null && item.QueryResult.TotalResultCount > 0)
            {
                int deisredLabelLength = 7;
                builder.AppendLine(string.Format("{0}", item.QueryResult.Results[0].FormattedID));
                builder.AppendLine("===");
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Title", deisredLabelLength), item.QueryResult.Results[0].Name));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Owner", deisredLabelLength), item.QueryResult.Results[0].Owner == null ? "No Owner" : item.QueryResult.Results[0].Owner._refObjectName));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Project", deisredLabelLength), item.QueryResult.Results[0].Project == null ? "No Project" : item.QueryResult.Results[0].Project._refObjectName));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("State", deisredLabelLength), item.QueryResult.Results[0].State));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Feature", deisredLabelLength), item.QueryResult.Results[0].Feature == null ? "Not associated with any feature" : item.QueryResult.Results[0].Feature._refObjectName));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Link", deisredLabelLength), GetLinkToItem(item.QueryResult.Results[0], "task")));
            }
            else
            {
                builder.Append(string.Format("Sorry, I could'nt find ta{0}", number));
            }

            return builder.ToString();
        }

        private static string GetStory(string number)
        {
            var builder = new StringBuilder();
            var url = string.Format("{0}hierarchicalrequirement.js?query=(FormattedId%20=%20de{1})&fetch=true", ApiUrl, number);
            var item = GetItem(url);

            if (item != null && item.QueryResult.TotalResultCount > 0)
            {
                int deisredLabelLength = 7;
                builder.AppendLine(string.Format("{0}", item.QueryResult.Results[0].FormattedID));
                builder.AppendLine("===");
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Title", deisredLabelLength), item.QueryResult.Results[0].Name));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Owner", deisredLabelLength), item.QueryResult.Results[0].Owner == null ? "No Owner" : item.QueryResult.Results[0].Owner._refObjectName));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Project", deisredLabelLength), item.QueryResult.Results[0].Project == null ? "No Project" : item.QueryResult.Results[0].Project._refObjectName));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("State", deisredLabelLength), item.QueryResult.Results[0].ScheduleState));
                builder.AppendLine(string.Format("{0} {1}", FormatLabel("Link", deisredLabelLength), GetLinkToItem(item.QueryResult.Results[0], "userstory")));
                builder.AppendLine();
                builder.AppendLine(FormatDescription(item.QueryResult.Results[0].Description.Value));
            }
            else
            {
                builder.Append(string.Format("Sorry, I could'nt find us{0}", number));
            }

            return builder.ToString();
        }

        private static string FormatDescription(string html)
        {
            var result = html;

            var regexBR = new Regex("(?i)(<br.*?>)");
            var regexP = new Regex("(?i)(<p.*?>)");
            var regexUL = new Regex("(?i)(<ul.*?>)");
            var regexLI = new Regex("(?i)(<li.*?>)");
            result = regexP.Replace(result, Environment.NewLine);
            result = regexBR.Replace(result, Environment.NewLine);
            result = regexUL.Replace(result, Environment.NewLine);
            result = regexLI.Replace(result, string.Format("{0}* ", Environment.NewLine));

            var regexHtml = new Regex("(?i)(<.+?>)");
            result = regexHtml.Replace(result, string.Empty);

            return result;
        }

        private static string FormatLabel(string label, int desiredLength)
        {
            return string.Format("[{0}]", label).PadRight(desiredLength + 2);
        }

        private static string GetLinkToItem(dynamic @object, string type)
        {
            var link = string.Empty;

            if (@object != null)
            {
                var objectId = @object.ObjectID.Value;
                string project = @object.Project._ref.Value;
                var projectId = project.Substring(project.LastIndexOf("/"));
                projectId = projectId.Substring(1, projectId.Length - 4);
                link = string.Format("https://rally1.rallydev.com/slm/rally.sp#/{0}/detail/{1}/{2}", projectId, type, objectId);
            }

            return link;
        }
    }
}