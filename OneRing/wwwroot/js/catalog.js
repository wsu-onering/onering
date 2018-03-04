///////////////////////////////////////////////////
// Configurable: VariableDropdown JS
///////////////////////////////////////////////////
let varDropdownConfig;
let configsAdded = [];


function startupVarDropdown(config, config_area) {
    // Config has all the options to populate the dropdown
    varDropdownConfig = config;

    // Config_area is the <div> to put the dropdown
    config_area.id = "dropdown_config_area";
    config_area.className = "dropdown";

    // Create html inside config_area
    let added_configs_area = document.createElement('div');
    added_configs_area.id = "added_configs_area";
    let dropdown_button = document.createElement('button');
    dropdown_button.id = "dropdown_button";
    dropdown_button.className = "add-config-button";
    dropdown_button.onclick = onDropdownClick;
    dropdown_button.innerHTML = "Add";
    let dropdown_item_selection = document.createElement('div');
    dropdown_item_selection.id = "dropdown_item_selection";
    dropdown_item_selection.className = "dropdown-content";
    for (option of varDropdownConfig.configFieldOptions) {
        addDropdownOption(dropdown_item_selection, option);
    }
    config_area.append(added_configs_area, dropdown_button, dropdown_item_selection);
}

function addDropdownOption(dropdown, option) {
    let item = document.createElement('a');
    item.id = "drop_item_" + option.id;
    item.innerHTML = option.name;
    item.onclick = onOptionAddClick(option);
    dropdown.append(item);
}

function onOptionRemoveClick(option) {
    return function () {
        // Remove item view
        document.getElementById("drop_item_" + option.id).remove();

        // Remove item from list of added
        for (let i = 0; i < configsAdded.length; i += 1) {
            if (configsAdded[i].id == option.id) {
                configsAdded.splice(i, 1);
                break;
            }
        }
        // Add item back into dropdown
        let dropdown = document.getElementById("dropdown_item_selection");
        addDropdownOption(dropdown, option);
    }
}

function onOptionAddClick(option) {
    return function () {
        // Remove item from dropdown
        document.getElementById("drop_item_" + option.id).remove();

        // Create name of config item and remove button
        let item = document.createElement('div');
        item.id = "drop_item_" + option.id;
        item.className = "add-item-button";
        let remove_btn = document.createElement('button');
        remove_btn.className = "remove-item-button";
        remove_btn.innerHTML = "Remove";
        remove_btn.onclick = onOptionRemoveClick(option);
        let option_name = document.createElement('h3');
        option_name.innerHTML = option.name;
        item.append(remove_btn, option_name);

        // Add to added configs area
        document.getElementById("added_configs_area").append(item);

        // Add to list to keep track
        configsAdded.push(option);
    }
}

/* When the user clicks on the button,
toggle between hiding and showing the dropdown content */
function onDropdownClick() {
    let element = document.getElementById("dropdown_item_selection");
    element.classList.toggle("show");
}

// Close the dropdown if the user clicks outside of it
window.onclick = function (event) {
    if (!event.target.matches('.add-config-button')) {
        let dropdown_content = document.getElementById("dropdown_item_selection");
        if (dropdown_content.classList.contains('show'))
            dropdown_content.classList.remove('show');
    }
}


///////////////////////////////////////////////////
// Configurable: SingleTextbox JS
///////////////////////////////////////////////////
let singleInputConfig;

function startupSingleInput(config, config_area) {
    // Config_area is the <div> to put the single input config
    config_area.id = "single_input_area";

    // Create html inside config_area
    let label = document.createElement('label');
    label.innerHTML = config.name + ":";
    let config_input = document.createElement('input');
    config_input.id = "single_config_input";
    config_input.type = "text";
    config_input.className = "config-input";
    config_area.append(label, config_input);
}



