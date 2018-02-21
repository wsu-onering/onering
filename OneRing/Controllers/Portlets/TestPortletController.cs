using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using onering.Models;

namespace onering.Controllers.Portlets
{
    public class TestPortletController : Controller
    {
        private readonly IConfiguration _configuration;
        private Database.IOneRingDB _db;
        private string PortletName = "WarpSpeed";
        private string PortletDescription = "Travel the stars at warp speed!";
        private string PortletIconPath = "https://placeimg.com/150/150/tech";

        public TestPortletController(IConfiguration configuration, Database.IOneRingDB db)
        {
            _configuration = configuration;
            _db = db;

            // Check if this portlet already exists in the Database, and if it isn't there, put it
            // into the database.
            bool weExist = false;
            foreach (Portlet portlet in _db.ListPortlets(this.PortletName)) {
                if (portlet.Name == this.PortletName) {
                    weExist = true;
                    Debug.WriteLine("We found a portlet with the same name as us: {0}, us {1}", portlet.Name, this.PortletName);
                }
            }
            if (weExist) {
                return;
            }
            Portlet p = new Portlet {
                Name = this.PortletName,
                Description = this.PortletDescription,
                Path = this.GetType().Name.Replace("Controller", ""),
                Icon = this.PortletIconPath,
            };
            this._db.CreatePortlet(p);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}