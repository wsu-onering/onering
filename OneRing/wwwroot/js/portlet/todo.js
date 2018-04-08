var lastColSorted;
var islastSortedDescending;

jQuery(document).ready(function () {
    resizeDiv();
    islastSortedDescending = true;
    lastColSorted = 'todo-date';
});

window.onresize = function (event) {
    resizeDiv();
}

function resizeDiv() {
    vph = $(window).height() - 80;
    $('div').css({ 'height': vph + 'px' });
}

function todoItemNav_onclick(link) {
    window.open(link);
}

function sortTable(columnClass, column) {
    // Check sorting ascending/descending
    let isDescending = (lastColSorted == columnClass) ? !islastSortedDescending : true;
    islastSortedDescending = isDescending;
    lastColSorted = columnClass;

    // Get sortable table headers
    let as = document.getElementsByClassName("col-header");

    // Hide all table header arrows
    for (let i = 0; i < as.length; ++i) {
        let span = as[i].childNodes[1];
        span.style.display = "none";
    }

    // Remove arrow
    let span = column.childNodes[1];
    let glyph = span.classList.contains('glyphicon-triangle-top') ? 'glyphicon-triangle-top' : 'glyphicon-triangle-bottom';
    span.classList.remove(glyph);

    // Add correct arrow, compare function, and show arrow
    let compareFunction;
    if (isDescending == true) {
        span.classList.add('glyphicon-triangle-bottom');
        compareFunction = a_LessThan_b;
    }
    else {
        span.classList.add('glyphicon-triangle-top');
        compareFunction = a_GreaterThan_b;
    }
    span.style.display = "inline";

    // Get rows by column that is being sorted
    let cols = document.getElementsByClassName(columnClass);
    let rows = [];

    // Populate array with value being sorted and row 
    for (let i = 0; i < cols.length; ++i) {
        let td = cols[i];
        rows.push({ value: td.dataset.content, row: td.parentElement });
    }
    
    // Remove rows from table
    let tableBody = document.getElementById("todo_tbody").remove();

    // Mergesort rows
    rows = mergeSort(rows, compareFunction);

    // Place rows back into table
    tableBody = document.createElement('tbody');
    tableBody.id = "todo_tbody";
    for (let i = 0; i < rows.length; ++i) {
        tableBody.appendChild(rows[i].row);
    }
    document.getElementById("todo_table").appendChild(tableBody);
}

// Split array into halves and merge recursively 
function mergeSort(arr, compareFunc) {
    if (arr.length === 1) {
        // Return after hold 1 element
        return arr
    }

    // Setup left/right arrays
    const middle = Math.floor(arr.length / 2);
    const left = arr.slice(0, middle);
    const right = arr.slice(middle);
    
    return merge(
        mergeSort(left, compareFunc),
        mergeSort(right, compareFunc),
        compareFunc
    )
}

// Sort and merge arrays
function merge(leftArray, rightArray, compareFunc) {
    let result = []
    let leftIndex = 0
    let rightIndex = 0

    while (leftIndex < leftArray.length && rightIndex < rightArray.length) {
        if (compareFunc(leftArray[leftIndex].value, rightArray[rightIndex].value)) {
            result.push(leftArray[leftIndex])
            leftIndex++
        }
        else {
            result.push(rightArray[rightIndex])
            rightIndex++
        }
    }

    return result.concat(leftArray.slice(leftIndex)).concat(rightArray.slice(rightIndex))
}

function a_LessThan_b(a, b) {
    return a < b;
}

function a_GreaterThan_b(a, b) {
    return a > b;
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