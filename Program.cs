using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace ThreadsHeadlines
{
    class Program
    {
        static void Main(string[] args)
        {
            var articles = GetArticlesFromPage("https://www.bbc.com/news/world", "li", "nw-c-nav__secondary-menuitem-container");

            foreach (var curr in articles)
            {
                Console.WriteLine(curr.Key + ": " + curr.Value);
            }
        }

        static Dictionary<string, string> GetArticlesFromPage(string url, string tag, string className)
        {
            var result = new Dictionary<string, string>();
            HtmlDocument HtmlDoc = new HtmlDocument();

            // Base url
            var uri = new Uri(url);
            string baseUrl = uri.Scheme + "://" + uri.Host;

            // Get the HTML from the url
            var client = new WebClient();
            var HtmlStr = client.DownloadString(url);
            HtmlDoc.LoadHtml(HtmlStr);

            // Get the relevant elements
            HtmlNodeCollection elements = HtmlDoc.DocumentNode.SelectNodes("//" + tag + "[contains(@class, '" + className + "')]");

            // Get the titles and the full links for the articles
            foreach (HtmlNode curr in elements)
            {
                string title = curr.InnerText;
                string link = curr.GetAttributeValue("href", "none") != "none" ? 
                              curr.GetAttributeValue("href", "none") : curr.ChildNodes["a"].GetAttributeValue("href", "none");
                string fullLink = link.StartsWith("/") ? baseUrl + link : link;
                result[title] = fullLink;
            }

            return result;
        }
    }
}
