using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MapTileService
{
    /// <summary>
    /// Various utility functions (for all types of tile sources)
    /// </summary>
    static class Utils
    {
        private static readonly string ImagePng = "image/png";

        private static readonly string ImageJpeg = "image/jpeg";

        private static readonly string LocalFileScheme = "file:///";

        private static readonly string MBTilesScheme = "mbtiles:///";

        public static TileSetConfiguration GetTileSetConfiguration(this IConfiguration configuration, string tilesetName)
        {
            return configuration
                .GetSection("tilesets")
                .Get<TileSetConfiguration[]>()
                .FirstOrDefault(ts => (String.Compare(ts.Name, tilesetName, StringComparison.Ordinal) == 0));
        }

        public static string GetContentType(string tileFormat)
        {
            var mediaType = String.Empty;
            switch (tileFormat)
            {
                case "png": { mediaType = ImagePng; break; }
                case "jpg": { mediaType = ImageJpeg; break; }
                default: throw new ArgumentException("tileFormat");
            }

            return mediaType;
        }

        public static bool IsMBTilesScheme(string source)
        {
            return (source.StartsWith(MBTilesScheme, StringComparison.Ordinal));
        }

        public static bool IsLocalFileScheme(string source)
        {
            return (source.StartsWith(LocalFileScheme, StringComparison.Ordinal));
        }

        public static string GetMBTilesConnectionString(string source)
        {
            var uriString = source.Replace(MBTilesScheme, LocalFileScheme, StringComparison.Ordinal);
            var uri = new Uri(uriString);

            return $"Data Source={uri.LocalPath}";
        }

        public static string GetLocalFilePath(string source, int x, int y, int z)
        {
            var uriString = String.Format(
                        CultureInfo.InvariantCulture,
                        source.Replace("{x}", "{0}").Replace("{y}", "{1}").Replace("{z}", "{2}"),
                        x,
                        y,
                        z);
            var uri = new Uri(uriString);

            return uri.LocalPath;
        }

        public static async Task<byte[]> ReadTileFromLocalFileAsync(TileSetConfiguration tileset, int x, int y, int z)
        {
            if (!tileset.Tms)
            {
                y = Utils.FromTmsY(y, z);
            }

            var path = Utils.GetLocalFilePath(tileset.Source, x, y, z);
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                var buffer = new byte[fileInfo.Length];
                using (var fileStream = fileInfo.OpenRead())
                {
                    await fileStream.ReadAsync(buffer, 0, buffer.Length);
                    return buffer;
                }
            }
            else
            {
                return null;
            }
        }

        public static async Task<byte[]> ReadTileFromMBTilesAsync(TileSetConfiguration tileset, int x, int y, int z)
        {
            if (!tileset.Tms)
            {
                y = Utils.FromTmsY(y, z);
            }

            var connectionString = Utils.GetMBTilesConnectionString(tileset.Source);
            var db = new MBTilesStorage(connectionString);
            var data = await db.GetTileAsync(x, y, z);
            return data;
        }

        // https://alastaira.wordpress.com/2011/07/06/converting-tms-tile-coordinates-to-googlebingosm-tile-coordinates/

        /// <summary>
        /// Convert Y tile coordinate of TMS standard (flip)
        /// </summary>
        /// <param name="y"></param>
        /// <param name="zoom">Zoom level</param>
        /// <returns></returns>
        public static int FromTmsY(int tmsY, int zoom)
        {
            return (1 << zoom) - tmsY - 1;
        }
    }
}
