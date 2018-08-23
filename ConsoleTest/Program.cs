using Wox.Plugin.GoogleSearch;

namespace ConsoleTest
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var gs = new GoogleSearch();
            var results = gs.Search("hello", 10);
        }
    }
}