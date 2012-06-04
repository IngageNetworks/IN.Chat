using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace IN.Chat.Web.Hubs.Services
{
    public static class TextTransformer
    {
        public static string FormatMultiLineContent(string content)
        {
            if (content.Contains('\n'))
            {
                return string.Format("<pre class=\"pre-scrollable\">{0}</pre>", content);
            }
            else
            {
                return content;
            }
        }

        public static string TransformAndExtractUrls(string content, out IEnumerable<Uri> urls)
        {
            const string urlPattern = @"(?i)(?<s>(?:https?|ftp)://|www\.)?(?:\S+(?::\S*)?@)?(?:(?:[\w\p{S}][\w\p{S}@-]*[.:])+\w+)(?(s)/?|/)(?:(?:[^\s()<>.,\u0022'”]+|[.,\u0022'”][^\s()<>,\u0022]|\((?:[^\s()<>]+|(?:\([^\s()<>]+\)))*\))*)";

            var extractedUrls = new List<Uri>();
            urls = new List<Uri>();

            try
            {
                content = Regex.Replace(content, urlPattern, m =>
                {
                    string httpPortion = String.Empty;
                    if (!m.Value.Contains("://"))
                    {
                        httpPortion = "http://";
                    }

                    string url = httpPortion + m.Value;

                    extractedUrls.Add(new Uri(HttpUtility.HtmlDecode(url)));

                    return String.Format(CultureInfo.InvariantCulture,
                                         "<a rel=\"nofollow external\" target=\"_blank\" href=\"{0}\" title=\"{1}\">{1}</a>",
                                         url, m.Value);
                });
                urls = extractedUrls;
            }
            catch
            {
            }

            return content;
        }

        public static string TransformUrls(string content)
        {
            IEnumerable<Uri> urls;
            return TransformAndExtractUrls(content, out urls);
        }
    }
}