using System;
using System.Threading.Tasks;

namespace MapTileService.TileSources
{
    class MBTilesTileSource : ITileSource
    {
        private TileSetConfiguration configuration;

        private readonly string contentType;

        public MBTilesTileSource(TileSetConfiguration configuration)
        {
            this.configuration = configuration;
            this.contentType = Utils.GetContentType(this.configuration.Format);
        }

        async Task<byte[]> ITileSource.GetTileAsync(int x, int y, int z)
        {
            if (!this.configuration.Tms)
            {
                y = Utils.FromTmsY(y, z);
            }

            var connectionString = GetMBTilesConnectionString(this.configuration.Source);
            var db = new MBTilesStorage(connectionString);
            var data = await db.GetTileAsync(x, y, z);
            return data;
        }

        private static string GetMBTilesConnectionString(string source)
        {
            var uriString = source.Replace(Utils.MBTilesScheme, Utils.LocalFileScheme, StringComparison.Ordinal);
            var uri = new Uri(uriString);

            return $"Data Source={uri.LocalPath}";
        }

        TileSetConfiguration ITileSource.Configuration => this.configuration;

        string ITileSource.ContentType => this.contentType;
    }
}
