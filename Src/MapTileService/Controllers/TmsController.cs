using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MapTileService.Controllers
{
    [Route("tms/1.0.0")]
    public class TmsController : Controller
    {
        private IConfiguration configuration;

        private static readonly string LocalFileScheme = "file:///";

        private static readonly string MBTilesScheme = "mbtiles:///";

        private static readonly string ImagePng = "image/png";

        private static readonly string ImageJpeg = "image/jpeg";

        public TmsController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // TODO: return capabilities documents (XML)

        /// <summary>
        /// Get tile from tileset with specified coordinates 
        /// URL format according to TMS specs, like http://localhost:50864/tms/1.0.0/world/3/4/5.png
        /// </summary>
        /// <param name="tilesetName">Tileset name</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z">Zoom level</param>
        /// <param name="formatExtension"></param>
        /// <returns></returns>
        [HttpGet("{tilesetName}/{z}/{x}/{y}.{formatExtension}")]
        public async Task<IActionResult> GetTile(string tilesetName, int x, int y, int z, string formatExtension)
        {
            var tileset = configuration
                .GetSection("tilesets")
                .Get<TileSetConfiguration[]>()
                .FirstOrDefault(ts => (String.Compare(ts.Name, tilesetName, StringComparison.Ordinal) == 0));

            if (tileset != null)
            {
                // TODO: check formatExtension == tileset.Format

                if (tileset.Source.StartsWith(MBTilesScheme, StringComparison.Ordinal))
                {
                    var connectionString = $"Data Source={tileset.Source.Substring(MBTilesScheme.Length)}";
                    var db = new MBTilesStorage(connectionString);
                    var data = await db.GetTileAsync(x, y, z);
                    if (data != null)
                    {
                        return File(data, GetContentType(tileset.Format));
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else if (tileset.Source.StartsWith(LocalFileScheme, StringComparison.Ordinal))
                {
                    var path = String.Format(
                        CultureInfo.InvariantCulture,
                        tileset.Source
                            .Substring(LocalFileScheme.Length)
                            .Replace("{x}", "{0}")
                            .Replace("{y}", "{1}")
                            .Replace("{z}", "{2}"),
                        x,
                        y,
                        z);

                    var fileInfo = new FileInfo(path);
                    var buffer = new byte[fileInfo.Length];
                    using (var fileStream = fileInfo.OpenRead())
                    {
                        await fileStream.ReadAsync(buffer, 0, buffer.Length);
                        return File(buffer, GetContentType(tileset.Format));
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return NotFound($"Specified tileset '{tilesetName}' not exists on server");
            }
        }

        private static string GetContentType(string tileFormat)
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
    }
}