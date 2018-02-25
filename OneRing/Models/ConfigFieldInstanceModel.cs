using System.Collections.Generic;

namespace onering.Models
{
    // ConfigFieldInstance models the information in a database for a given instance of a
    // configuration field ("configuration field" is represented by ConfigField), hence the name
    // "ConfigFieldInstance".
    // Note: Because ConfigFieldInstance needs "points" to several different items in the database,
    // the model has fields which are populated with models representing the tables being
    // referenced. This is done so that we need-not write DB methods like
    // "ListConfigFieldInstanceForConfigFieldOption"; instead, the "ConfigFieldOption" field will be
    // populated automatically.
    public class ConfigFieldInstance
    {
        // ID is the unique identifier for this ConfigFieldInstance
        public int ID { get; set; }

        // ConfigFieldInstanceValue is the Value of this instance of a configfield.
        public string ConfigFieldInstanceValue { get; set; }

        // The ConfigField that this ConfigFieldInstance is pointing at. This field may be null. If
        // it is null, then this ConfigFieldInstance points at a ConfigFieldOption instead, and you
        // can find that ConfigFieldOption in the ConfigFieldOption field of this class.
        public ConfigField ConfigField { get; set; }

        // The ConfigFieldOption that this ConfigFieldInstance is pointing at. This field may be
        // null. If it is null, then this ConfigFieldInstance points at a ConfigField instead, and
        // you can find that ConfigField in the ConfigField field of this class.
        public ConfigFieldOption ConfigFieldOption { get; set; }
        public PortletInstance PortletInstance { get; set; }
    }
}