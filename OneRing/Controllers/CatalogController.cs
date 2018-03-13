using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

using onering.Models;

namespace onering.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IConfiguration _configuration;
        private Database.IOneRingDB _db;

        public CatalogController(IConfiguration configuration, Database.IOneRingDB db)
        {
            _configuration = configuration;
            _db = db;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View(_db.ListPortlets());
        }

        public IEnumerable<Portlet> Portlet()
        {
            return _db.ListPortlets();
        }

        // Creates a new portlet instance for a given user
        [Authorize]
        [HttpPost]
        public IActionResult PortletInstance(PortletInstance pi)
        {
            if (pi == null)
                return null;

            // Get user
            string id = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            OneRingUser user = _db.ListOneRingUsers(id)[0];

            // Setup configurations
            if (pi.ConfigFieldInstances != null) {
                foreach (ConfigFieldInstance configInstance in pi.ConfigFieldInstances) {
                    if (configInstance.PortletInstance == null)
                        configInstance.PortletInstance = pi;
                }
            }
            // Set user
            pi.User = user;

            // Create portlet instance
            _db.CreatePortletInstance(pi);

            return Json(new Dictionary<int, int>());
        }
    }
}
