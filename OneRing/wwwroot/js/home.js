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


///////////////////////////////////////////////////
// Entrypoint for this file
///////////////////////////////////////////////////
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
    // disableIframe disables interacting with the iframe portion of portlet by inserting an sibling
    // element which lays atop that iframe. The positioning of this element is handled in css, but
    // this overlay element must come before the iframe in the containing div for the overlay to
    // work.
    function disableIframe(element) {
        //console.log("disabling iframe", element);
        let overlay = document.createElement('div');
        overlay.setAttribute('class', 'portlet-overlay');
        let body = $(element).find('.portlet-body')[0];
        body.prepend(overlay);
        //console.log('body: ', body);
    }
    // enableIframe enables interacting with the iframe portion of a portlet by removing the
    // `overlay` element within the gridstack 'window'.
    function enableIframe(element) {
        //console.log("Enabling iframe!", element);
        let overlay = $(element).find('.portlet-overlay')[0];
        overlay.remove();
    }

    $(function () {
        $('.grid-stack').gridstack(options);
        $('.grid-stack').on('change', (event, items) => {
            //console.log(event, items);
            saveDimensions(items);
        });
        // Manage iframe interactivity within the portlet being manipulated. When the user starts to
        // either drage or resize a portlet, disable the iframe of within that portlet. Then, when
        // the user stops dragging or resizing a portlet, re-enable the iframe within that portlet.
        $('.grid-stack').on('resizestart', function(event, ui) {
            let element = event.target;
            disableIframe(element);
        });
        $('.grid-stack').on('resizestop', function(event, ui) {
            let element = event.target;
            enableIframe(element);
        });
        $('.grid-stack').on('dragstart', function(event, ui) {
            var element = event.target;
            disableIframe(element);
        });
        $('.grid-stack').on('dragstop', function(event, ui) {
            var element = event.target;
            enableIframe(element);
        });
    });
});


///////////////////////////////////////////////////
// Saving portlets' positions/sizes
///////////////////////////////////////////////////
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

    // Send to controller
    $.ajax({
        dataType: "json",
        url: "/Home/Update",
        method: "POST",
        data: { '': portletInstances }
    });
}


///////////////////////////////////////////////////
// Delete portlet
///////////////////////////////////////////////////
function deletePortlet_OnClick(instanceID) {
    // Send to controller
    $.ajax({
        dataType: "json",
        url: "/Home/Delete",
        method: "POST",
        data: { portletInstanceID: instanceID } 
    });

    // Refresh home page
    location.reload();
}