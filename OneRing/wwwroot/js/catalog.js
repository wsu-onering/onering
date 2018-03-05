///////////////////////////////////////////////////
// Catalog JS
///////////////////////////////////////////////////
let portletSelected;

function onCatalogAddClick() {
    if (portletSelected == null)
        return;

    // Create PortletInstance from portlet
    let portletInstance = {
        Portlet: portletSelected,
        ConfigFieldInstances: [],
        Height: 1,
        Width: 1,
        XPos: 0,
        YPos: 0
    };

    // Grab all configurations
    for (config of portletSelected.configFields) {
        switch (config.id) {
            // Variable dropdown config 
            case 1:         
                for (option of varDropdownsAdded) {
                    portletInstance.ConfigFieldInstances.push(createConfigInstance(option, option.name));
                }
                break;
            // Free form input config
            case 2:         
                let config_input = document.getElementById("single_config_input");
                portletInstance.ConfigFieldInstances.push(createConfigInstance(singleInputConfig, config_input.value));
                break;
        }
    }

    $.ajax({
        type: "POST",
        url: "/Catalog/PortletInstance",
        data: portletInstance
    });
    window.location.href = "/";
}

function onCatalogCancelClick() {
    window.location.href = "/";
}

function createConfigInstance(config, value) {
    return {
        ConfigField: config,
        ConfigFieldInstanceValue: value
    }
}

function onPortletSelected(portlet, desc_area) {
    return function () {
        console.log(portlet, desc_area);

        // Set selected portlet
        portletSelected = portlet;

        // Enable add portlet button
        let add_portlet_btn = document.getElementById("add_portlet_btn");
        if (add_portlet_btn.classList.contains('disabled')) {
            add_portlet_btn.classList.remove('disabled');
        }

        // Clear last portlet description area
        desc_area.empty();

        // Create new portlet description area
        let header = document.createElement('h1');
        header.innerHTML = portlet.name;
        let desc = document.createElement('p');
        desc.innerHTML = portlet.description;
        desc_area.append(header, desc);
        var config_area = document.createElement('div');
        for (config of portlet.configFields) {
            let header = document.createElement('h1');
            header.innerHTML = config.name;
            let desc = document.createElement('p');
            desc.innerHTML = config.description;
            let config_item = document.createElement('div');
            switch (config.id) {
                // Variable dropdown config 
                case 1:         
                    startupVarDropdown(config, config_item);
                    break;
                // Free form input config
                case 2:         
                    startupSingleInput(config, config_item);
                    break;
            }
            config_area.append(header, desc, config_item);
        }
        desc_area.append(config_area);
    }
}

function attach_click_funcs() {
    $.get("/Catalog/Portlet").done((data) => {
        let domPortlets = $(".portlet-list-item");
        let portletdivs = {};
        domPortlets.each((index, elem) => {
            let name = $(elem).find(".portlet-list-item-name").text();
            portletdivs[name] = $(elem);
        });
        console.log(portletdivs);
        console.log(domPortlets);
        // Register a click event for each portlet
        for (portlet of data) {
            console.log(portlet);
            let desc_area = $("#portlet-description");
            let div = portletdivs[portlet.name];
            div.click(onPortletSelected(portlet, desc_area));
        }
    });
}

$(attach_click_funcs);




///////////////////////////////////////////////////
// Configurable: VariableDropdown JS
///////////////////////////////////////////////////
let varDropdownConfig;
let varDropdownsAdded = [];

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
    dropdown_button.className = "add-config-button btn btn-success btn-lg";
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
        for (let i = 0; i < varDropdownsAdded.length; i += 1) {
            if (varDropdownsAdded[i].id == option.id) {
                varDropdownsAdded.splice(i, 1);
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
        remove_btn.className = "remove-item-button btn btn-danger btn-lg";
        remove_btn.innerHTML = "Remove";
        remove_btn.onclick = onOptionRemoveClick(option);
        let option_name = document.createElement('h3');
        option_name.innerHTML = option.name;
        item.append(remove_btn, option_name);

        // Add to added configs area
        document.getElementById("added_configs_area").append(item);

        // Add to list to keep track
        varDropdownsAdded.push(option);
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
    singleInputConfig = config;

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



