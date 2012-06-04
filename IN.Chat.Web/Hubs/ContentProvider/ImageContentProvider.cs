using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using IN.Chat.Web.Hubs.Entities;
using IN.Chat.Web.Hubs.Services;

namespace IN.Chat.Web.Hubs.ContentProvider.Core
{
    [InheritedExport]
    public class ImageContentProvider : EmbeddedContentProvider
    {
        public override string Name { get { return "Images"; } }
        public override string Description { get { return "Turns image urls into embedded images."; } }

        private bool IsImageUrl(Uri url)
        {
            return url.AbsoluteUri.ToLower().EndsWith(".png") ||
                url.AbsoluteUri.ToLower().EndsWith(".bmp") ||
                url.AbsoluteUri.ToLower().EndsWith(".jpg") ||
                url.AbsoluteUri.ToLower().EndsWith(".jpeg") ||
                url.AbsoluteUri.ToLower().EndsWith(".gif");
        }

        public override bool ContainsContent(Message message)
        {
            IEnumerable<Uri> urls;
            TextTransformer.TransformAndExtractUrls(message.ProcessedContent, out urls);
            return urls.Any(u => IsImageUrl(u));
        }

        public override void ExtractContent(Message message)
        {
            IEnumerable<Uri> urls;
            TextTransformer.TransformAndExtractUrls(message.ProcessedContent, out urls);

            foreach (var url in urls.Distinct().Where(u => IsImageUrl(u)))
            {
                var extractedContent = string.Format("<img style=\"max-height:500px\" src=\"{0}\" />", url);
                message.ProcessedContent += string.Format(base.EmbeddedContentMarkUpFormat, url, extractedContent);
            }
        }
    }
}