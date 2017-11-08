using Microsoft.Data.Sqlite;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace MapTileService
{
    public class MBTilesStorage
    {
        private readonly string connectionString;

        public MBTilesStorage(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<byte[]> GetTileAsync(int x, int y, int z)
        {
            return await this.ReadTileDataAsync(x, y, z).ConfigureAwait(false);
        }

        private const string TableTiles = "tiles";

        private static int TileRow(int y, int zoom)
        {
            return ((2 << zoom) / 2 - y - 1);
        }

        private async Task<byte[]> ReadTileDataAsync(int x, int y, int z)
        {
            var commandText = String.Format(CultureInfo.InvariantCulture,
                "SELECT tile_data FROM {0} WHERE ((zoom_level = @zoom_level) AND (tile_column = @tile_column) AND (tile_row = @tile_row))",
                TableTiles);

            byte[] result = null;
            using (var connection = new SqliteConnection(this.connectionString))
            {
                using (var command = new SqliteCommand(commandText, connection))
                {
                    command.Parameters.AddRange(new[]
                    {
                        new SqliteParameter("@tile_column", x),
                        new SqliteParameter("@tile_row", TileRow(y, z)),
                        new SqliteParameter("@zoom_level", z),
                    });

                    await connection.OpenAsync().ConfigureAwait(false);
                    using (var dr = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (await dr.ReadAsync().ConfigureAwait(false))
                        {
                            result = ((byte[])dr[0]);
                        }

                        dr.Close();
                    }
                }

                connection.Close();
            }

            return result;
        }
    }
}
