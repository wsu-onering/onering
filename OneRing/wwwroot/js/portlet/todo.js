function setup() {
    console.log("This is the script for todoportlet");
    console.log($('tr'));
    for (row of $('tr')) {
        var button = $(row).find('button');
        if (row.attributes.length > 0) {
            button.click(todoClicked(row));
        }
    }
}

function todoClicked(row) {
    return function () {
        var sourceID = row.attributes.sourceid.value;
        var userID = row.attributes.userid.value;
        var itemID = row.attributes.itemid.value;
        console.log("the button for the row was clicked for row", row);
        var data = JSON.stringify({
            "sourceID": sourceID,
            "userID": userID,
            "itemID": itemID
        });
        console.log("data:", data);
        $(row).hide();
        $.ajax({
            "method": "POST",
            "data": data,
            'contentType': 'application/json',
            'url': '/TodoPortlet/MarkDone'
        }).done(() => {
            console.log("We sent the thing off and we hope it went well!");
            //location.reload();
        }).fail((stuff) => {
            console.log("Apparently everything failed : (");
            console.log(stuff);
            $(row).show();
        });
    }
}