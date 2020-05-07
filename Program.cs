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
            var articles = WebPage.GetArticlesFromPage("https://www.bbc.com/news/world", "li", "nw-c-nav__secondary-menuitem-container");
            
            //Dictionary<string, Dictionary<string, string>> allArticles = GetArticlesFromPageByQ(articles);

            foreach (var curr in articles)
            {
                Console.WriteLine(curr.Key + ": " + curr.Value);
            }
        }

        static Dictionary<string, Dictionary<string, string>> GetArticlesFromPageByQ(Dictionary<string, string> urls) 
        {
            Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();
            ThreadPool.SetMaxThreads(10, urls.Count);

            foreach (var curr in urls.Keys) {
                //var currPage = new WebPage()
                //{
                //    url = urls[curr],
                //    tag = "li",
                //    className = "nw-c-nav__secondary-menuitem-container"
                //};
                result[curr] = WebPage.GetArticlesFromPage(urls[curr], "li", "nw-c-nav__secondary-menuitem-container");
                //ThreadPool.QueueUserWorkItem(new WaitCallback(GetArticlesFromPage), currPage);
            }

            return result;
        }
    }

    class WebPage
    {
        private ManualResetEvent _doneEvent;

        public string Url { get; }
        public string Tag { get; }
        public string ClassName { get; }

        public Dictionary<string, string> Articles { get; private set; }

        public WebPage(string url, string tag, string className, ManualResetEvent doneEvent)
        {
            Url = url;
            Tag = tag;
            ClassName = className;
            _doneEvent = doneEvent;
        }

        public void ThreadPoolCallback(Object threadContext)
        {
            Articles = GetArticlesFromPage(Url, Tag, ClassName);
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
                string link = curr.GetAttributeValue("href", "none") != "none" ?
                              curr.GetAttributeValue("href", "none") : curr.ChildNodes["a"].GetAttributeValue("href", "none");
                string fullLink = link.StartsWith("/") ? baseUrl + link : link;
                result[title] = fullLink;
            }

            return result;
        }
    }
}
