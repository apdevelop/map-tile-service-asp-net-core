﻿using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MapTileService.Controllers
{
    [Route("api/tiles")]
    public class TilesController : Controller
    {
        private readonly IConfiguration configuration;

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
        public async Task<IActionResult> GetTile1Async(string tileset, int x, int y, int z)
        {
            return await this.ReadTileAsync(tileset, x, y, z);
        }

        [HttpGet("{tileset}/{z}/{x}/{y}")]
        public async Task<IActionResult> GetTile2Async(string tileset, int x, int y, int z)
        {
            return await ReadTileAsync(tileset, x, y, z);
        }

        private async Task<IActionResult> ReadTileAsync(string tileset, int x, int y, int z)
        {
            if (!String.IsNullOrEmpty(tileset))
            {
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
                    return NotFound($"Specified tileset '{tileset}' not found on server");
                }
            }
            {
                return BadRequest();
            }
        }
    }
}
