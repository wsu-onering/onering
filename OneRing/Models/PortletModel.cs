using System.Collections.Generic;

namespace onering.Models
{
	public class Portlet
	{
        // ID is the unique ID for this portlet.
        public int ID { get; set; }
        // Name is the name of the portlet which is shown for this portlet in the catalog.
        public string Name {get; set;}
        // Descript is a description of the Portlet. This description will be displayed to the user
        // in the portlet catalog.
        public string Description {get; set;}
        // Path is the path to the view for this portlet. Each portlet has a single main 'view' path.
        public string Path {get; set;}
        // Icon represents the http(s) path to the icon used to represent this portlet. This may be
        // a local or a remote path.
        public string Icon {get; set;}
        // ConfigFields is a list of all the config fields associated with this portlet.
        public List<ConfigField> ConfigFields {get; set;}
	}
}