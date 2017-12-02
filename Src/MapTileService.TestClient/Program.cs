using System;
using System.Diagnostics;
using System.Net.Http;

namespace MapTileService.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var count = 100;
            var zoomLevel = 10;

            var sw = Stopwatch.StartNew();
            GetRandomTiles("digitalglobe", zoomLevel, count);
            sw.Stop();

            Console.WriteLine("Read {0} tiles in {1}  (Average: {2:0.0} ms)",
                count,
                sw.Elapsed,
                sw.Elapsed.TotalMilliseconds / ((double)count));
        }

        private static void GetRandomTiles(string tilesetName, int zoomLevel, int count)
        {
            var random = new Random();

            // !!! There is very low performance issue with HttpClient in .NET Core
            // https://github.com/dotnet/corefx/issues/24104
            // https://www.reddit.com/r/csharp/comments/54iygj/httpclients_sendasync_is_very_slow_not_a_proxy/

            var handler = new HttpClientHandler() { UseProxy = false }; // Workaround for that issue
            var client = new HttpClient(handler);

            for (var i = 0; i < count; i++)
            {
                // TODO: base address and other options to config/parameters
                var x = random.Next(0, (int)Math.Pow(2, zoomLevel));
                var y = random.Next(0, (int)Math.Pow(2, zoomLevel));
                var z = zoomLevel;
               
                var uri = $"http://localhost:5000/tms/1.0.0/{tilesetName}/{z}/{x}/{y}.jpg";

                try
                {
                    var tileContents = client.GetByteArrayAsync(uri).Result;
                }
                catch (AggregateException)
                {
                    // Ignore 404 errors
                }
            }

            client.Dispose();
        }
    }
}
