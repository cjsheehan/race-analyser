using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace RacingWebScraper
{
    public static class WebPage
    {
        public static async Task<String> GetAsync(string uri)
        {
            if(String.IsNullOrWhiteSpace(uri)) throw new ArgumentNullException("String uri cannot be empty or null");
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            string page = await client.GetStringAsync(uri).ConfigureAwait(false);
            return page;
        }

        public static async Task<AngleSharp.Dom.IDocument> GetDocumentAsync(string uri)
        {
            if (String.IsNullOrWhiteSpace(uri)) throw new ArgumentNullException("String uri cannot be empty or null");
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            string page = await client.GetStringAsync(uri).ConfigureAwait(false);
            return new AngleSharp.Parser.Html.HtmlParser().Parse(page);
        }

        public static string Get(string uri)
        {
            if (String.IsNullOrWhiteSpace(uri)) throw new ArgumentNullException("String uri cannot be empty or null");

            string page = null;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Encoding responseEncoding = Encoding.GetEncoding(response.CharacterSet);
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), responseEncoding))
            {
                page = reader.ReadToEnd();
            }

            if (String.IsNullOrEmpty(page)) throw new ArgumentNullException(String.Format("Page cannot be null : uri {0}", uri));
            return page;
        }
    }

}
