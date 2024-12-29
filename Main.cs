using Flow.Launcher.Plugin;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wox.Plugin.GoogleSearch
{
    public class Main: IAsyncPlugin
    {
        private GoogleSearch _gs;

        private PluginInitContext _context;

        public async Task<List<Result>> QueryAsync(Query query, CancellationToken token) {
            await Task.Delay(300, token);
            var results = new List<Result>();
            if (string.IsNullOrEmpty(query.Search)) return results;
            var searchResults = await _gs.Search(query.Search, 8, token);
            foreach (var s in searchResults)
            {
                var r = new Result
                {
                    Title = s.Name,
                    SubTitle = s.DecodedUrl,
                    IcoPath = @"images\icon.png",
                    Action = c =>
                    {
                        try
                        {
                            _context.API.OpenUrl(s.Url);
                            return true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    },
                };
                results.Add(r);
            }

            return results;
        }

        public async Task InitAsync(PluginInitContext context)
        {
            _context = context;
            _gs = new GoogleSearch();
        }
    }
}
