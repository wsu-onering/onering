using System.Collections.Generic;

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
        public List<ConfigFieldInstance> ConfigFieldInstances {get; set;}
        public int Height {get; set;}
        public int Width {get; set;}
        public int XPos {get; set;}
        public int YPos {get; set;}

    }
}