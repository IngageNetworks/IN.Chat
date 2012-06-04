using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using IN.Chat.Web.Hubs.Entities;
using IN.Chat.Web.Hubs.Services;

namespace IN.Chat.Web.Hubs.ContentProvider.Core
{
    [InheritedExport]
    public class YouTubeContentProvider : EmbeddedContentProvider
    {
        private static readonly Regex YoutubeRegex = new Regex(
            @"# Match non-linked youtube URL in the wild. (Rev:20111012)
            https?://         # Required scheme. Either http or https.
            (?:[0-9A-Z-]+\.)? # Optional subdomain.
            (?:               # Group host alternatives.
              youtu\.be/      # Either youtu.be,
            | youtube\.com    # or youtube.com followed by
              \S*             # Allow anything up to VIDEO_ID,
              [^\w\-\s]       # but char before ID is non-ID char.
            )                 # End host alternatives.
            ([\w\-]{11})      # $1: VIDEO_ID is exactly 11 chars.
            (?=[^\w\-]|$)     # Assert next char is non-ID or EOS.
            (?!               # Assert URL is not pre-linked.
              [?=&+%\w]*      # Allow URL (query) remainder.
              (?:             # Group pre-linked alternatives.
                [\'""][^<>]*> # Either inside a start tag,
              | </a>          # or inside <a> element text contents.
              )               # End recognized pre-linked alts.
            )                 # End negative lookahead assertion.
            [?=&+%\w-]*       # Consume any URL (query) remainder.",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public override string Name { get { return "YouTube"; } }
        public override string Description { get { return "Turns YouTube urls into embedded videos."; } }

        public override bool ContainsContent(Message message)
        {
            IEnumerable<Uri> urls;
            TextTransformer.TransformAndExtractUrls(message.ProcessedContent, out urls);

            return urls.Any(u => u.Authority.Equals("www.youtube.com", StringComparison.OrdinalIgnoreCase) ||
                u.Authority.Equals("youtu.be", StringComparison.OrdinalIgnoreCase));
        }

        public override void ExtractContent(Message message)
        {
            const string extractedContentFormat = @"<object width=""425"" height=""344""><param name=""WMode"" value=""transparent""></param><param name=""movie"" value=""http://www.youtube.com/v/{0}fs=1""></param><param name=""allowFullScreen"" value=""true""></param><param name=""allowScriptAccess"" value=""always""></param><embed src=""http://www.youtube.com/v/{0}?fs=1"" wmode=""transparent"" type=""application/x-shockwave-flash"" allowfullscreen=""true"" allowscriptaccess=""always"" width=""425"" height=""344B""></embed></object>";

            IEnumerable<Uri> urls;
            TextTransformer.TransformAndExtractUrls(message.ProcessedContent, out urls);

            foreach (var url in urls.Distinct())
            {
                Match match = YoutubeRegex.Match(url.ToString());
                if (match.Groups.Count < 2 || String.IsNullOrEmpty(match.Groups[1].Value))
                {
                }
                else
                {
                    string videoId = match.Groups[1].Value;
                    string extractedContent = string.Format(extractedContentFormat, videoId);
                    message.ProcessedContent += string.Format(base.EmbeddedContentMarkUpFormat, url, extractedContent);
                }
            }
        }
    }
}