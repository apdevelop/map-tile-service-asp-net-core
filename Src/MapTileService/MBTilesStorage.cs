using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;

namespace MapTileService
{
    public class MBTilesStorage
    {
        private const string TableTiles = "tiles";

        private readonly string connectionString;

        public MBTilesStorage(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<byte[]> GetTileAsync(int tileColumn, int tileRow, int zoomLevel)
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
                        new SqliteParameter("@tile_column", tileColumn),
                        new SqliteParameter("@tile_row", tileRow),
                        new SqliteParameter("@zoom_level", zoomLevel),
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

        public async Task<byte[]> GetTileAsync(long rowId)
        {
            var commandText = String.Format(CultureInfo.InvariantCulture,
                "SELECT tile_data FROM {0} WHERE (rowid = @rowId)",
                TableTiles);

            byte[] result = null;
            using (var connection = new SqliteConnection(this.connectionString))
            {
                using (var command = new SqliteCommand(commandText, connection))
                {
                    command.Parameters.Add(new SqliteParameter("@rowId", rowId));

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

        public static long CreateTileCoordinatesKey(long zoomLevel, long tileColumn, long tileRow)
        {
            // Z[8] | X[24] | Y[24]
            return (long)((zoomLevel << 48) | ((tileColumn & 0xFFFFFF) << 24) | (tileRow & 0xFFFFFF));
        }

        public async Task ReadTileCoordinates(ConcurrentDictionary<long, long> tileKeys)
        {
            var commandText = $"SELECT rowid, zoom_level, tile_column, tile_row FROM {TableTiles}";

            using (var connection = new SqliteConnection(this.connectionString))
            {
                using (var command = new SqliteCommand(commandText, connection))
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    using (var dr = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await dr.ReadAsync().ConfigureAwait(false))
                        {
                            tileKeys.TryAdd(CreateTileCoordinatesKey(dr.GetInt64(1), dr.GetInt64(2), dr.GetInt64(3)), dr.GetInt64(0));
                        }

                        dr.Close();
                    }
                }

                connection.Close();
            }
        }
    }
}
