using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MapTileService.MBTiles
{
    /// <summary>
    /// Data access layer for MBTiles 1.2 (SQLite) database
    /// https://github.com/mapbox/mbtiles-spec/blob/master/1.2/spec.md
    /// </summary>
    public class MBTilesStorage
    {
        private const string TableTiles = "tiles";

        /// <summary>
        /// MBTiles tileset magic number
        /// https://www.sqlite.org/src/artifact?ci=trunk&filename=magic.txt
        /// </summary>
        private const int ApplicationId = 0x4d504258;

        /// <summary>
        /// Connection string for SQLite database
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">Connection string for SQLite database</param>
        public MBTilesStorage(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<byte[]> GetTileAsync(int tileColumn, int tileRow, int zoomLevel)
        {
            var commandText = $"SELECT tile_data FROM {TableTiles} WHERE ((zoom_level = @zoom_level) AND (tile_column = @tile_column) AND (tile_row = @tile_row))";
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
            var commandText = $"SELECT tile_data FROM {TableTiles} WHERE (rowid = @rowId)";
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

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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

        public async Task<IList<long>> ReadTileCoordinatesKeys()
        {
            var result = new List<long>();

            var commandText = $"SELECT zoom_level, tile_column, tile_row FROM {TableTiles}";
            using (var connection = new SqliteConnection(this.connectionString))
            {
                using (var command = new SqliteCommand(commandText, connection))
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    using (var dr = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await dr.ReadAsync().ConfigureAwait(false))
                        {
                            result.Add(CreateTileCoordinatesKey((int)dr.GetInt64(0), (int)dr.GetInt64(1), (int)dr.GetInt64(2)));
                        }

                        dr.Close();
                    }
                }

                connection.Close();
            }

            return result;
        }

        public async Task InsertTileDataAsync(int tileColumn, int tileRow, int zoomLevel, byte[] tileData, long timestamp)
        {
            var commandText = $"INSERT INTO {TableTiles} (tile_column, tile_row, zoom_level, tile_data, timestamp) VALUES (@tile_column, @tile_row, @zoom_level, @tile_data, @timestamp); SELECT last_insert_rowid();";

            using (var connection = new SqliteConnection(this.connectionString))
            {
                using (var command = new SqliteCommand(commandText, connection))
                {
                    command.Parameters.AddRange(CreateParamsArray(tileColumn, tileRow, zoomLevel, tileData, timestamp));
                    await connection.OpenAsync().ConfigureAwait(false);
                    var rowid = (long)(await command.ExecuteScalarAsync().ConfigureAwait(false));
                }

                connection.Close();
            }
        }

        private static SqliteParameter[] CreateParamsArray(int tileColumn, int tileRow, int zoomLevel, byte[] tileData, long timestamp)
        {
            return new[]
            {
                new SqliteParameter("@tile_column", tileColumn),
                new SqliteParameter("@tile_row", tileRow),
                new SqliteParameter("@zoom_level", zoomLevel),
                new SqliteParameter("@tile_data", tileData),
                new SqliteParameter("@timestamp", timestamp),
            };
        }
    }
}
