using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace ThreadsHeadlines
{
    class Program
    {
        static void Main(string[] args)
        {
            // Final dictionary to check out the results
            Dictionary<string, Dictionary<string, string>> allArticles = new Dictionary<string, Dictionary<string, string>>();

            // Get the continents pages' urls
            var articles = WebPage.GetArticlesFromPage("https://www.bbc.com/news/world", "li", "nw-c-nav__secondary-menuitem-container");

            var doneEvents = new ManualResetEvent[articles.Count];
            var pagesArray = new WebPage[articles.Count];

            // Maximum 10 threads at a time
            ThreadPool.SetMaxThreads(10, 10);

            int i = 0;

            // Build requests queue and execute the calls
            foreach (var curr in articles.Keys)
            {
                doneEvents[i] = new ManualResetEvent(false);
                var page = new WebPage(curr, articles[curr], "a", "gs-c-promo-heading", doneEvents[i]);
                pagesArray[i] = page;
                ThreadPool.QueueUserWorkItem(page.ThreadPoolCallback, i);
                i++;
            }
            
            // Wait for all of the IO to be finished
            WaitHandle.WaitAll(doneEvents);

            // Print the results and add them to a structure
            for (int j = 0; j < articles.Count; j++)
            {
                WebPage currPage = pagesArray[j];

                // Set the structure
                allArticles[currPage.Category] = currPage.Articles;

                // Print the results
                Console.WriteLine("-------------------------------------------------------");
                Console.WriteLine(currPage.Category);

                foreach (var currArticle in currPage.Articles)
                {
                    Console.WriteLine(currArticle.Key + ": " + currArticle.Value);
                }
            }
        }
    }

    class WebPage
    {
        private ManualResetEvent _doneEvent;

        public string Category { get; }
        public string Url { get; }
        public string Tag { get; }
        public string ClassName { get; }

        public Dictionary<string, string> Articles { get; private set; }

        public WebPage(string category, string url, string tag, string className, ManualResetEvent doneEvent)
        {
            Category = category;
            Url = url;
            Tag = tag;
            ClassName = className;
            _doneEvent = doneEvent;
        }

        public void ThreadPoolCallback(Object threadContext)
        {
            // Gets the articles from the givven page
            Articles = GetArticlesFromPage(Url, Tag, ClassName);

            // Signal that all done :)
            _doneEvent.Set();
        }

        public static Dictionary<string, string> GetArticlesFromPage(string url, string tag, string className)
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
                string link = curr.GetAttributeValue("href", "none");

                // Check if the link is found - if not get child element for it
                link = link != "none" ? link : curr.ChildNodes["a"].GetAttributeValue("href", "none");

                // Check if the link is relative
                string fullLink = link.StartsWith("/") ? baseUrl + link : link;
                result[title] = fullLink;
            }

            return result;
        }
    }
}
