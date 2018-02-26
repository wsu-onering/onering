///////////////////////////////////////////////////
// Configuratable: VariableDropdown JS
///////////////////////////////////////////////////
let varDropdownConfig;
let configsAdded = [];

function createDropdownOption(option) {
    let item = document.createElement('a');
    item.id = "drop-item-" + option.id;
    item.innerHTML = option.name;
    item.onclick = onOptionAddClick(option);
    return item;
}

function onOptionRemoveClick(option) {
    return function () {
        // Remove item view
        document.getElementById("drop-item-" + option.id).remove();
        // Remove item from list of added
        for (let i = 0; i < configsAdded.length; i += 1) {
            if (configsAdded[i].id == option.id) {
                configsAdded.splice(i, 1);
                break;
            }
        }
        // Add item back into dropdown
        let add_dropdown = document.getElementById("dropdownItems");
        add_dropdown.append(createDropdownOption(option));
    }
}

function onOptionAddClick(option) {
    return function () {
        // Remove item from dropdown
        document.getElementById("drop-item-" + option.id).remove();
        // Create name of config and remove button
        let item = document.createElement('div');
        item.id = "drop-item-" + option.id;
        item.className = "addedDropItem";
        let remove_btn = document.createElement('button');
        remove_btn.className = "removeItemButton";
        remove_btn.innerHTML = "Remove";
        remove_btn.onclick = onOptionRemoveClick(option);
        let option_name = document.createElement('h3');
        option_name.innerHTML = option.name;
        item.append(remove_btn, option_name);
        // Add to added configs area
        document.getElementById("addedConfigsArea").append(item);
        // Add to list to keep track
        configsAdded.push(option);
    }
}

function startupVarDropdown() {
    let add_dropdown = document.getElementById("dropdownItems");
    for (option of varDropdownConfig.configFieldOptions) {
        add_dropdown.append(createDropdownOption(option));
    }
}

/* When the user clicks on the button,
toggle between hiding and showing the dropdown content */
function onDropdownClick() {
    document.getElementById("dropdownItems").classList.toggle("show");
}

// Close the dropdown if the user clicks outside of it
window.onclick = function (event) {
    if (!event.target.matches('.addItemButton')) {
        var dropdowns = document.getElementsByClassName("dropdown-content");
        var i;
        for (i = 0; i < dropdowns.length; i++) {
            var openDropdown = dropdowns[i];
            if (openDropdown.classList.contains('show')) {
                openDropdown.classList.remove('show');
            }
        }
    }
}


///////////////////////////////////////////////////
// Configuratable: SingleTextbox JS
///////////////////////////////////////////////////

