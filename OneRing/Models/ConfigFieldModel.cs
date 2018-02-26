using System.Collections.Generic;
using System.Data.SqlClient;

namespace onering.Models
{
    public class ConfigField
    {
        // ID is the unique ID for this ConfigField.
        public int ID { get; set; }
        // Name is the name of the this field, shown to the user.
        public string Name { get; set; }
        // Description is a description of this field for configuration purposes.
        public string Description { get; set; }
        public List<ConfigFieldOption> ConfigFieldOptions;

        
        public static ConfigField ReadFromDb(SqlDataReader reader)
        {
            return new ConfigField {
                ID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2)
            };
        }
    }
}