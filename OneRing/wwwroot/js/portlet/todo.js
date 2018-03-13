jQuery(document).ready(function () {
    console.log("This is the script for todoportlet");
    console.log($('tr'));
    for (row of $('tr')) {
        var navButton = $(row).find('input');
        if (row.attributes.length > 0) {
            navButton.click(navTodoItemClick(row));
        }
    }
});

function navTodoItemClick(row) {
    return function () {
        var todoLink = row.attributes.link.value;
        window.open(todoLink);
    }
}

//function removeTodoItemClick(row) {
//    return function () {
//        var sourceID = row.attributes.sourceid.value;
//        var userID = row.attributes.userid.value;
//        var itemID = row.attributes.itemid.value;
//        console.log("the button for the row was clicked for row", row);
//        var data = JSON.stringify({
//            "sourceID": sourceID,
//            "userID": userID,
//            "itemID": itemID
//        });
//        console.log("data:", data);
//        $(row).hide();
//        $.ajax({
//            "method": "POST",
//            "data": data,
//            'contentType': 'application/json',
//            'url': '/TodoPortlet/MarkDone'
//        }).done(() => {
//            console.log("We sent the thing off and we hope it went well!");
//        }).fail((stuff) => {
//            console.log("Apparently everything failed : (");
//            console.log(stuff);
//            $(row).show();
//        });
//    }
//}