using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace onering.Models
{
    /// <summary>
    /// While a bare portlet represents the implementation of a portlet, a PortletInstance represents
    /// a reference to a portlet implementation as well as a collection of configuration values
    /// required by that portlet so that it may render correctly for the user associated with it
    /// through a PortletInstance.
    /// </summary>
    public class PortletInstance
    {
        // ID is the internal identifier of this PortletInstance.
        public int ID { get; set; }

        // The Portlet that this PortletInstance is an instance of
        public Portlet Portlet { get; set; }

        // The User that this PortletInstance belongs to.
        public OneRingUser User { get; set; }

        // A collection of the ConfigFieldInstances associated with this PortletInstance. This field
        // may be null if the portlet this is an instance of requires no configuration.
        public List<ConfigFieldInstance> ConfigFieldInstances { get; set; }

        // Position and dimensions of PortletInstance
        public int Height { get; set; }
        public int Width { get; set; }
        public int XPos { get; set; }
        public int YPos { get; set; }


        public static PortletInstance ReadFromDb(SqlDataReader reader)
        {
            return new PortletInstance {
                ID = reader.GetInt32(0),
                Height = reader.GetInt32(3),
                Width = reader.GetInt32(4),
                XPos = reader.GetInt32(5),
                YPos = reader.GetInt32(6)
            };
        }
    }
}