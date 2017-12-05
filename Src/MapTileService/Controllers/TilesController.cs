using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
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
            if (String.IsNullOrEmpty(tileset))
            {
                return BadRequest();
            }

            if (Startup.TileSources.ContainsKey(tileset))
            {
                var tileSource = Startup.TileSources[tileset];
                var data = await tileSource.GetTileAsync(x, Utils.FromTmsY(y, z), z);
                if (data != null)
                {
                    return File(data, tileSource.ContentType);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound($"Specified tileset '{tileset}' not exists on server");
            }
        }
    }
}
