using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Wox.Plugin.GoogleSearch
{
    public class Main: IPlugin
    {
        private string _pluginDirectory;

        private GoogleSearch _gs; 

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();
            if (string.IsNullOrEmpty(query.Search)) return results;
            var searchResults = _gs.Search(query.Search, 8);
            foreach (var s in searchResults)
            {
                var r = new Result
                {
                    Title = s.Name,
                    SubTitle = s.Url,
                    IcoPath = $"{_pluginDirectory}\\images\\icon.png",
                    Action = c =>
                    {
                        try
                        {
                            Process.Start(s.Url);
                            return true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    },
                    ContextData = s
                };
                results.Add(r);
            }

            return results;
        }

        public void Init(PluginInitContext context)
        {
            _pluginDirectory = context.CurrentPluginMetadata.PluginDirectory;
            _gs = new GoogleSearch();
            
        }
    }
}