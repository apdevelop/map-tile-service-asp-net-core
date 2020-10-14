# Map Tile Service
Minimal implementation of Tile Map Service (TMS) for .NET Core 2.1; provides access to map raster (PNG/JPEG) tiles stored in standard MBTiles database and local file system.

### Demo page
![Demo page](https://github.com/apdevelop/map-tile-service-asp-net-core/blob/master/Docs/demo-page.png)
 
### Technologies
Developed using MS Visual Studio 2017 (with .NET Core SDK 2.1.518), C#, ASP.Net Core 2.1.
Using
* [Microsoft.Data.Sqlite](https://github.com/aspnet/Microsoft.Data.Sqlite) for working with SQLite database
* [Leaflet](https://github.com/Leaflet) for map demo page

### Creating SCD for running on Linux (Raspbian OS)

Install `libunwind8` package on target system to run .NET Core applications:

`sudo apt-get install libunwind8`

On development system with VS2017 in the solution (`...\Src`) directory execute the following command:

`dotnet publish -c Release -r linux-arm`

It will create the self-contained deployment (SCD) so that target system don't need to have .NET Core Runtime installed.

Output files will be placed into folder:

`...\Src\MapTileService\bin\Release\netcoreapp2.1\linux-arm\publish\`

Copy contents of this directory to target system (for example into `/home/pi/tileservice` directory), set permissions (execute access) to startup file (note that filename is case-sensitive and has no extension)

`chmod +x MapTileService`

Run application (file without extension):

`./MapTileService`

After start, it will listen on default TCP port 5000 (using in-process `Kestrel` web server) 
and tile service with demo page will be available on http://localhost:5000/ address; to enable remote calls allow connections to this port in firewall settings.

### Some sample datasets
* [World Countries MBTiles by ArcGIS / EsriAndroidTeam](https://www.arcgis.com/home/item.html?id=7b650618563741ca9a5186c1aa69126e)
* [Satellite Lowres raster tiles Planet by OpenMapTiles](https://openmaptiles.com/downloads/dataset/satellite-lowres/)

### TODOs
* Better implementation of MBTiles specifications (processing metadata, corner cases).
* Support for more formats (vector tiles).
* Include test dataset created from free data.
* Access to metadata.
* Live demo.

### References
* [Tile Map Service](https://en.wikipedia.org/wiki/Tile_Map_Service)
* [Tile Map Service Specification](https://wiki.osgeo.org/index.php?title=Tile_Map_Service_Specification)
* [MBTiles Specification](https://github.com/mapbox/mbtiles-spec)
* [Using TMS in Leaflet](http://leafletjs.com/examples/wms/wms.html)