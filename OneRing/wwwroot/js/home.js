// Because this file is significantly smaller than it's dependencies, the only way we can make sure
// that this file is loaded last is using a javascript module/loading system. The standard is to use
// `require.js`, and what you see below is configuration telling requirejs where to load modules
// from.
requirejs.config({
    baseUrl: "js",
    paths: {
        jquery: "https://ajax.googleapis.com/ajax/libs/jquery/3.3.1/jquery",
        lodash: "https://cdnjs.cloudflare.com/ajax/libs/lodash.js/4.17.5/lodash",
        "jquery-ui": "/lib/jquery-ui",
        gridstack: 'https://cdnjs.cloudflare.com/ajax/libs/gridstack.js/0.3.0/gridstack',
        'gridstack.jQueryUI': 'https://cdnjs.cloudflare.com/ajax/libs/gridstack.js/0.3.0/gridstack.jQueryUI.min',
    },
    shim: {
        'gridstack.jQueryUI': ['gridstack', 'jquery']
    }
});


/////
//  Entrypoint for this file
/////
require(['lodash', 'jquery', 'gridstack.jQueryUI', 'gridstack'], function(_, $, ____){
    // Called once all the required libraries are loaded
    "use strict";

    var options = {
        rtl: true,
        float: true,
        gridwidth: 6,
        draggable: {
            handle: '.portlet-heading',
        }
    };

    $(function () {
        $('.grid-stack').gridstack(options);
        $('.grid-stack').on('change', (event, items) => {
            console.log(event, items);
            saveDimensions(items);
        });
    });
});


/////
//  Saving portlets' positions/sizes
/////
function saveDimensions(items) {
    let portletInstances = [];

    // Setup PortletInstances that changed
    for (let i = 0; i < items.length; ++i) {
        let p = items[i];
        portletInstances.push({
            ID: parseInt(p.id),
            Height: p.height,
            Width: p.width,
            XPos: p.x,
            YPos: p.y
        });
    }

    // Post to controller
    $.ajax({
        dataType: "json",
        url: "/Home/Update",
        method: "POST",
        data: { '': portletInstances }
    });
}



