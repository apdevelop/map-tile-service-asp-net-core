using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace MapTileService.TileSources
{
    class MBTilesTileSource : ITileSource
    {
        private TileSetConfiguration configuration;

        private readonly string contentType;

        /// <summary>
        /// [Tile coordinates; row id]
        /// </summary>
        private ConcurrentDictionary<long, long> tileKeys = new ConcurrentDictionary<long, long>();

        private bool isTileKeysReady = false;

        public MBTilesTileSource(TileSetConfiguration configuration)
        {
            this.configuration = configuration;
            this.contentType = Utils.GetContentType(this.configuration.Format); // TODO: from db metadata

            if (this.configuration.UseCoordinatesCache)
            {
                var filePath = GetLocalFilePath(this.configuration.Source);
                if (File.Exists(filePath))
                {
                    // TODO: not the best placement in constructor
                    Task.Run(() =>
                    {
                        var connectionString = GetMBTilesConnectionString(this.configuration.Source);
                        var db = new MBTilesRepository(connectionString);
                        db.ReadTileCoordinatesAsync(this.tileKeys).Wait(); // TODO: check presence of rowid
                        this.isTileKeysReady = true;
                    });
                }
            }
        }

        async Task<byte[]> ITileSource.GetTileAsync(int x, int y, int z)
        {
            if (!this.configuration.Tms)
            {
                y = Utils.FromTmsY(y, z);
            }

            var connectionString = GetMBTilesConnectionString(this.configuration.Source);
            var db = new MBTilesRepository(connectionString);

            // TODO: if database contents were changed, coordinates cache should be invalidated

            if (this.configuration.UseCoordinatesCache)
            {
                var key = MBTilesRepository.CreateTileCoordinatesKey(z, x, y);
                if (tileKeys.ContainsKey(key))
                {
                    // Get rowid from cache, read table record by rowid (very fast, compared to selecting by three columns)
                    return await db.ReadTileAsync(tileKeys[key]);
                }
                else
                {
                    if (this.isTileKeysReady)
                    {
                        // Assuming there is no tile in database, if it not exists in cache
                        return null;
                    }
                    else
                    {
                        // While cache is not ready, allow simple database read
                        return await db.ReadTileAsync(x, y, z);
                    }
                }
            }
            else
            {
                return await db.ReadTileAsync(x, y, z);
            }
        }

        private static string GetLocalFilePath(string source)
        {
            var uriString = source.Replace(Utils.MBTilesScheme, Utils.LocalFileScheme, StringComparison.Ordinal);
            var uri = new Uri(uriString);

            return uri.LocalPath;
        }

        private static string GetMBTilesConnectionString(string source)
        {
            // https://github.com/aspnet/Microsoft.Data.Sqlite/wiki/Connection-Strings

            return $"Data Source={GetLocalFilePath(source)}; Mode=ReadOnly;";
        }

        TileSetConfiguration ITileSource.Configuration => this.configuration;

        string ITileSource.ContentType => this.contentType;
    }
}
