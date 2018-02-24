
using System.Collections.Generic;

namespace onering.Models
{
    public class OneRingUser
    {
        // ID is the unique identifier for this User within OneRing. This ID is only to identify
        // this user and their associated settings within the OneRing dashboard; do not send this ID
        // to remote datasources, use the GraphID of this user instead.
        public int ID { get; set; }

        // GraphID is the unique identifier for this User outside of OneRing. It is safe to use this
        // to identify the user with external services, such as the Microsoft graph or custom
        // portlet datastores.
        public string GraphID { get; set; }

        // All the instances of portlets for this user, that show up on the home page of this user.
        // This field may or may not be set, as it can lead to huge amounts of DB queries and huge
        // responses.
        public List<PortletInstance> PortletInstances { get; set; }

    }
}