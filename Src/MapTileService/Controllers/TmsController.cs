using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MapTileService.Controllers
{
    [Route("tms/1.0.0")]
    public class TmsController : Controller
    {
        private IConfiguration configuration;

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

                var connectionString = String.Format(CultureInfo.InvariantCulture, "Data Source={0}", tileset.Source.Substring("mbtiles:///".Length));
                var db = new MBTilesStorage(connectionString);
                var data = await db.GetTileAsync(x, y, z);
                if (data != null)
                {
                    var mediaType = String.Empty;
                    switch (tileset.Format)
                    {
                        case "png": { mediaType = ImagePng; break; }
                        case "jpg": { mediaType = ImageJpeg; break; }
                        default: throw new ArgumentException("format");
                    }

                    return File(data, mediaType);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }
    }
}