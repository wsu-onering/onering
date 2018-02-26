using System.Collections.Generic;
using System.Data.SqlClient;

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
        // ID of owner ConfigField
        public int ConfigFieldID { get; set; }


        public static ConfigFieldOption ReadFromDb(SqlDataReader reader)
        {
            ConfigFieldOption cfo = new ConfigFieldOption();
            cfo.ID = reader.GetInt32(0);
            cfo.Name = reader.GetString(1);
            cfo.Value = reader.GetString(2);
            cfo.ConfigFieldID = reader.GetInt32(3);
            return cfo;
        }
    }
}