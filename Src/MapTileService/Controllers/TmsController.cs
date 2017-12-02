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
            if (Startup.TileSources.ContainsKey(tilesetName))
            {
                // TODO: check formatExtension == tileset.Configuration.Format
                var tileSource = Startup.TileSources[tilesetName];
                var data = await tileSource.GetTileAsync(x, y, z);
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
                return NotFound($"Specified tileset '{tilesetName}' not exists on server");
            }
        }
    }
}