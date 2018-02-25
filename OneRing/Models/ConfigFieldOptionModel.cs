using System.Collections.Generic;

namespace onering.Models
{
    public class ConfigFieldOption
    {
        // ID is the unique ID for this ConfigFieldOption.
        public int ID { get; set; }
        // Name is the name of this ConfigFieldOption which is shown in the catalog.
        public string Name { get; set; }
        // Value is the value of this ConfigFieldOption
        public string Value { get; set; }
    }
}