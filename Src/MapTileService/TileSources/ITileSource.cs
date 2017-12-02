using System.Threading.Tasks;

namespace MapTileService.TileSources
{
    interface ITileSource
    {
        Task<byte[]> GetTileAsync(int x, int y, int z);

        TileSetConfiguration Configuration { get; }

        string ContentType { get; }
    }
}
