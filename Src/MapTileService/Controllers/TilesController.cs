using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

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

        // http://localhost:50864/api/tiles?x=0&y=0&z=0
        [HttpGet]
        public async Task<IActionResult> GetTile(int x, int y, int z)
        {
            var db = new MBTilesStorage(configuration["ConnectionStrings:MBTilesConnection"]);
            var data = await db.GetTileAsync(x, y, z);
            if (data != null)
            {
                return File(data, "image/png");
            }
            else
            {
                return NotFound();
            }
        }
    }
}