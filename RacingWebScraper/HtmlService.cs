using System;
using System.Threading.Tasks;

namespace RacingWebScraper
{
    public static class HtmlService
    {
        private static readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();
        private static readonly AngleSharp.Parser.Html.HtmlParser _htmlParser = new AngleSharp.Parser.Html.HtmlParser();
        public static async Task<AngleSharp.Dom.IDocument> GetDocumentAsync(string uri)
        {
            if (String.IsNullOrWhiteSpace(uri)) throw new ArgumentNullException("uri is empty or null");
            String page = await _httpClient.GetStringAsync(uri).ConfigureAwait(false);
            return  await _htmlParser.ParseAsync(page).ConfigureAwait(false);
        }

        public static String GetCanonicalUrl(AngleSharp.Dom.IDocument document)
        {
            if (document == null) throw new ArgumentNullException("document is null");
            var url = document.QuerySelector("link[rel='canonical']").GetAttribute("href");
            if (url == null)
            {
                return "";
            }
            else
            {
                return url;
            }
        }
    }
}
