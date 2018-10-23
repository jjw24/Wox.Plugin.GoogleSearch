using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace Wox.Plugin.GoogleSearch
{
    public class GoogleSearch
    {
        // Inspired heavily by: https://github.com/aviaryan/alfred-google-search/blob/master/src/gsearch/googlesearch.py
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                                         "(KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";

        private readonly HttpClient _client;

        public GoogleSearch()
        {
            _client = new HttpClient();
            // Set random user agent string
            _client.DefaultRequestHeaders.UserAgent.Clear();
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        }

        public IEnumerable<GoogleSearchResult> Search(string query, int limit)
        {
            query = query.Replace(' ', '+');
            var response = _client.GetAsync(BuildSearchUri(query));
            var results = ParseResponseWithHAP(response.Result).ToList();
            
            return results.Take(limit);
        }

        private static IEnumerable<GoogleSearchResult> ParseResponseWithHAP(HttpResponseMessage response)
        {
            var htmlDoc = new HtmlDocument();
            var googleSearchResults = new List<GoogleSearchResult>();
            
            htmlDoc.LoadHtml(response.Content.ReadAsStringAsync().Result);

            var allElementsWithClassR = htmlDoc.QuerySelectorAll("div.r");

            
            foreach (var e in allElementsWithClassR)
            {
                var link = e.QuerySelector("a").Attributes.First(a => a.Name == "href").Value;
                var title = e.QuerySelector("h3").InnerText;
                Console.WriteLine("Title: " + title);
                Console.WriteLine("Link: " + link);

                googleSearchResults.Add(new GoogleSearchResult()
                {
                    Name = title,
                    Url = link
                });
            }

            return googleSearchResults;
        }
        
        private static IEnumerable<GoogleSearchResult> ParseResponse(HttpResponseMessage response)
        {
            var headerRegex = new Regex("<h3.*?>.*?</h3>", RegexOptions.IgnoreCase);
            var linkRegex = new Regex(".*?[\\s*]href=\"(.*?)\".*?>(.*?)</a>.*$", RegexOptions.IgnoreCase);

            var searchResults = headerRegex.Matches(response.Content.ReadAsStringAsync().Result);
            var googleSearchResults = new List<GoogleSearchResult>();
            foreach (var sr in searchResults)
            {
                var match = linkRegex.Match(sr.ToString());
                if (!match.Success)
                {
                    continue;
                }

                var url = match.Groups[1];
                
                googleSearchResults.Add(new GoogleSearchResult()
                {
                    Name = HttpUtility.UrlDecode(match.Groups[2].Value),
                    Url = Regex.Replace(url.Value, "<.*?>", "")
                });

            }

            return googleSearchResults;
        }

        private static string BuildSearchUri(string query)
        {
            var builder = new UriBuilder("https://www.google.com/search");
            var queryString = HttpUtility.ParseQueryString(builder.Query);
            queryString["q"] = query;
            builder.Query = queryString.ToString();
            var url = builder.ToString();
            return url; 
        }
    }
}