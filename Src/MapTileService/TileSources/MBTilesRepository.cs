using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;

namespace MapTileService.TileSources
{
    /// <summary>
    /// Data access layer for MBTiles 1.2 (SQLite) database
    /// </summary>
    /// <remarks>
    /// See https://github.com/mapbox/mbtiles-spec/blob/master/1.2/spec.md
    /// </remarks>
    class MBTilesRepository
    {
        // Using pure ADO.NET instead of ORM for performance reason

        /// <summary>
        /// MBTiles tileset magic number
        /// </summary>
        /// <remarks>
        /// See https://www.sqlite.org/src/artifact?ci=trunk&filename=magic.txt
        /// </remarks>
        private const int ApplicationId = 0x4d504258;

        /// <summary>
        /// Connection string for SQLite database
        /// </summary>
        private readonly string connectionString;

        #region Database objects names

        private const string TableTiles = "tiles";

        private const string ColumnTileColumn = "tile_column";

        private const string ColumnTileRow = "tile_row";

        private const string ColumnZoomLevel = "zoom_level";

        private const string ColumnTileData = "tile_data";

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">Connection string for SQLite database</param>
        public MBTilesRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<byte[]> ReadTileAsync(int tileColumn, int tileRow, int zoomLevel)
        {
            var commandText = $"SELECT {ColumnTileData} FROM {TableTiles} WHERE (({ColumnZoomLevel} = @zoom_level) AND ({ColumnTileColumn} = @tile_column) AND ({ColumnTileRow} = @tile_row))";
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
                            result = (byte[])dr[0];
                        }

                        dr.Close();
                    }
                }

                connection.Close();
            }

            return result;
        }

        public async Task<byte[]> ReadTileAsync(long rowId)
        {
            var commandText = $"SELECT {ColumnTileData} FROM {TableTiles} WHERE (rowid = @rowId)";
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
                            result = (byte[])dr[0];
                        }

                        dr.Close();
                    }
                }

                connection.Close();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CreateTileCoordinatesKey(long zoomLevel, long tileColumn, long tileRow)
        {
            // Z[8] | X[24] | Y[24]
            return (long)((zoomLevel << 48) | ((tileColumn & 0xFFFFFF) << 24) | (tileRow & 0xFFFFFF));
        }

        public async Task ReadTileCoordinatesAsync(ConcurrentDictionary<long, long> tileKeys)
        {
            var commandText = $"SELECT rowid, {ColumnZoomLevel}, {ColumnTileColumn}, {ColumnTileRow} FROM {TableTiles}";
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

        public async Task<IList<long>> ReadTileCoordinatesKeysAsync()
        {
            var result = new List<long>();

            var commandText = $"SELECT {ColumnZoomLevel}, {ColumnTileColumn}, {ColumnTileRow} FROM {TableTiles}";
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
    }
}
