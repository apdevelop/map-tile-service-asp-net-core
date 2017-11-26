using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace MapTileService.Controllers
{
    [Route("tms/1.0.0")]
    public class TmsController : Controller
    {
        private IConfiguration configuration;

        public TmsController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // TODO: return capabilities documents (XML)

        /// <summary>
        /// Get tile from tileset with specified coordinates 
        /// URL format according to TMS 1.0.0 specs, like http://localhost/MapTileService/tms/1.0.0/world/3/4/5.png
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
            var tileset = configuration.GetTileSetConfiguration(tilesetName);
            if (tileset != null)
            {
                // TODO: check formatExtension == tileset.Format
                if (Utils.IsMBTilesScheme(tileset.Source))
                {
                    var data = await Utils.ReadTileFromMBTilesAsync(tileset, x, y, z);
                    if (data != null)
                    {
                        return File(data, Utils.GetContentType(tileset.Format));
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else if (Utils.IsLocalFileScheme(tileset.Source))
                {
                    var data = await Utils.ReadTileFromLocalFileAsync(tileset, x, y, z);
                    if (data != null)
                    {
                        return File(data, Utils.GetContentType(tileset.Format));
                    }
                    else
                    {
                        return NotFound();
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
    }
}