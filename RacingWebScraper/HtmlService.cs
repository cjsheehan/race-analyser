using AngleSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RacingWebScraper
{

    public static class HtmlService
    {
			private static readonly log4net.ILog log =
log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();
        private static readonly AngleSharp.Parser.Html.HtmlParser _htmlParser = new AngleSharp.Parser.Html.HtmlParser();

        public static async Task<AngleSharp.Dom.IDocument> GetDocumentAsync(string uri)
        {
            if (String.IsNullOrWhiteSpace(uri)) throw new ArgumentNullException("uri is empty or null");
						String page = null;

            try
            {
							 page = await _httpClient.GetStringAsync(new Uri(uri)).ConfigureAwait(false);
            }
            catch (System.Exception e)
            {
							log.Error(String.Format("Failed to get page : {0}, {1}", uri, e.Message));
            }
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
