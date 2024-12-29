using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

namespace Wox.Plugin.GoogleSearch
{
    public class GoogleSearch
    {
        // Inspired heavily by: https://github.com/aviaryan/alfred-google-search/blob/master/src/gsearch/googlesearch.py
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:133.0) Gecko/20100101 Firefox/133.0";

        private readonly HttpClient _client;

        public GoogleSearch()
        {
            _client = new HttpClient();
            // Set random user agent string
            _client.DefaultRequestHeaders.UserAgent.Clear();
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        }

        public async Task<IEnumerable<GoogleSearchResult>> Search(string query, int limit, CancellationToken token)
        {
            query = query.Replace(' ', '+');

            var response = await _client.GetAsync(BuildSearchUri(query), token);
            var results = await ParseResponseWithHAP(response, token);

            return results.Take(limit);
        }

        private static async Task<IEnumerable<GoogleSearchResult>> ParseResponseWithHAP(HttpResponseMessage response, CancellationToken token)
        {
            var htmlDoc = new HtmlDocument();
            var googleSearchResults = new List<GoogleSearchResult>();

            htmlDoc.LoadHtml(await response.Content.ReadAsStringAsync(token));

            var allElementsWithClassG = htmlDoc.QuerySelectorAll("div.g");


            foreach (var e in allElementsWithClassG)
            {
                var link = e.QuerySelector("a").Attributes.FirstOrDefault(a => a.Name == "href")?.Value;
                var title = e.QuerySelector("h3")?.InnerText;

                if (link == null || title == null)
                    continue;

                title = HtmlEntity.DeEntitize(title);

                googleSearchResults.Add(new GoogleSearchResult
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
