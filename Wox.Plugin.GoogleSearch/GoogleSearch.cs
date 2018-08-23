using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

namespace Wox.Plugin.GoogleSearch
{
    public class GoogleSearch
    {
        // Inspired heavily by: https://github.com/aviaryan/alfred-google-search/blob/master/src/gsearch/googlesearch.py
        private const string UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 " + 
                                         "(KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

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
            var results = ParseResponse(response.Result).ToList();
            
            return results.Take(limit);
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