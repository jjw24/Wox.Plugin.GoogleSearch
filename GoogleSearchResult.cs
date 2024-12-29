using System.Web;

namespace Wox.Plugin.GoogleSearch
{
    public class GoogleSearchResult
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string DecodedUrl => HttpUtility.UrlDecode(Url);
    }
}
