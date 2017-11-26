using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace MapTileService.Controllers
{
    [Route("api/tiles")]
    public class TilesController : Controller
    {
        private IConfiguration configuration;

        public TilesController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // TODO: use shared code for processing tile request

        /// <summary>
        /// Get tile from tileset with specified coordinates 
        /// URL in Google Maps format, like http://localhost/MapTileService/?tileset=world&x=4&y=5&z=3
        /// </summary>
        /// <param name="tileset">Tileset name</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z">Zoom level</param>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IActionResult> GetTile(string tileset, int x, int y, int z)
        {
            var tilesetConfiguration = configuration.GetTileSetConfiguration(tileset);
            if (tilesetConfiguration != null)
            {
                y = Utils.FromTmsY(y, z);

                if (Utils.IsMBTilesScheme(tilesetConfiguration.Source))
                {
                    var data = await Utils.ReadTileFromMBTilesAsync(tilesetConfiguration, x, y, z);
                    if (data != null)
                    {
                        return File(data, Utils.GetContentType(tilesetConfiguration.Format));
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else if (Utils.IsLocalFileScheme(tilesetConfiguration.Source))
                {
                    var data = await Utils.ReadTileFromLocalFileAsync(tilesetConfiguration, x, y, z);
                    if (data != null)
                    {
                        return File(data, Utils.GetContentType(tilesetConfiguration.Format));
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
                return NotFound($"Specified tileset '{tileset}' not exists on server");
            }
        }
    }
}
