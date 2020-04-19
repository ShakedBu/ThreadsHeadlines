using System;
using System.IO;
using System.Net;

namespace ThreadsHeadlines
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        static void GetArticlesFromPage(string url, string tag, string className)
        {
            // Get the HTML
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // If we got a result - not an error
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (String.IsNullOrWhiteSpace(response.CharacterSet))
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, System.Text.Encoding.UTF8);

                string HTML = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }
        }
    }
}
