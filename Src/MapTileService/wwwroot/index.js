(function () {

    // TODO: update Tile Grid overlay after base map change
    // TODO: get tilesets list from TMS capabilities response

    var baseMaps = {
        'world': L.tileLayer('/tms/1.0.0/satellite-lowres/{z}/{x}/{y}.png', {
            attribution: 'OpenMapTiles',
            maxZoom: 5,
            tms: true
        }),
        'world-fs': L.tileLayer('/tms/1.0.0/world-fs/{z}/{x}/{y}.png', {
            attribution: 'Esri.com',
            maxZoom: 5,
            tms: true
        }),
        'digitalglobe (TMS)': L.tileLayer('/tms/1.0.0/digitalglobe/{z}/{x}/{y}.jpg', {
            attribution: '',
            maxZoom: 17,
            tms: true
        }),
        'digitalglobe (not TMS)': L.tileLayer('/api/tiles/?tileset=digitalglobe&x={x}&y={y}&z={z}', {
            attribution: '',
            maxZoom: 17
        }),
        'world-countries': L.tileLayer('/tms/1.0.0/world-countries/{z}/{x}/{y}.png', {
            attribution: 'Esri.com',
            maxZoom: 5,
            tms: true
        }),
        'digitalglobe-cache': L.tileLayer('/tms/1.0.0/digitalglobe-cache/{z}/{x}/{y}.jpg', {
            attribution: '',
            maxZoom: 17,
            tms: true
        }),
        'RPi.world': L.tileLayer('/tms/1.0.0/world-rpi/{z}/{x}/{y}.png', {
            attribution: 'Esri.com',
            maxZoom: 5,
            tms: true
        }),
        'RPi.world-fs': L.tileLayer('/tms/1.0.0/world-fs-rpi/{z}/{x}/{y}.png', {
            attribution: 'Esri.com',
            maxZoom: 5,
            tms: true
        }),
        'RPi.digitalglobe (not TMS)': L.tileLayer('/api/tiles/?tileset=digitalglobe-rpi&x={x}&y={y}&z={z}', {
            attribution: '',
            maxZoom: 17
        })
    };

    var tileGrid = L.gridLayer.tileGrid({
        opacity: 1.0,
        zIndex: 2,
        pane: 'overlayPane'
    });

    var overlayMaps = {
        'Tile Grid': tileGrid
    };

    var map = L.map('map', {
        inertia: false,
        doubleClickZoom: false,
        layers: [baseMaps['world']]
    }).setView([0, 0], 0);


    L.control.layers(baseMaps, overlayMaps, {
        collapsed: false
    }).addTo(map);

})();