# Map Tile Service
Minimal implementation of Tile Map Service (TMS); provides access to map raster tiles stored in standard MBTiles database and local file system.

### Demo page
![Demo page](https://github.com/apdevelop/map-tile-service-asp-net-core/blob/master/Docs/demo-page.png)
 
### Technologies
Developed using MS Visual Studio 2017, C#, ASP.Net Core 2.0.
Using
* [Microsoft.Data.Sqlite](https://github.com/aspnet/Microsoft.Data.Sqlite) for working with SQLite database
* [Leaflet](https://github.com/Leaflet) for map demo page

### Running on Raspberry Pi (Raspbian OS) step-by-step

Install `libunwind8` package on RPi to run .NET Core applications:

`sudo apt-get install libunwind8`

On development system with VS2017 in the solution (`...\Src`) directory execute the following command:

`dotnet publish -c Release -r linux-arm`

It will create the self-contained deployment (SCD) so that target system don't need to have .NET Core Runtime installed.

Output files will be placed into

`...\Src\MapTileService\bin\Release\netcoreapp2.0\linux-arm\publish\`

Copy contents of this directory to RPi (for example into `/home/pi/tileservice` directory), set permissions (execute access) to startup file (note that filename is case-sensitive and has no extension)

`chmod +x MapTileService`

Run application (file without extension):

`./MapTileService`

After start, it will listen on default TCP port 5000 (using in-process Kestrel web server) and tile service with demo page will be available on http://localhost:5000/ address; to enable remote calls adjust firewall settings.

### References
* [Tile Map Service](https://en.wikipedia.org/wiki/Tile_Map_Service)
* [Tile Map Service Specification](https://wiki.osgeo.org/index.php?title=Tile_Map_Service_Specification)
* [MBTiles tileset format](https://github.com/mapbox/mbtiles-spec)
* [WMS in Leaflet](http://leafletjs.com/examples/wms/wms.html)