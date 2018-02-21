
using onering.Models;

namespace onering.Models.Interfaces {
    public interface IPortlet
    {
        string PortletName {get;}
        string PortletDescription {get;}
        string PortletIconPath {get;}
        string PortletPath {get;}
    }
}