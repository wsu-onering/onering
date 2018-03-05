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
        public IActionResult PortletInstance(PortletInstance pi) {
            string id = this.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            OneRingUser user = _db.ListOneRingUsers(id)[0];
            // TODO: Do some validation on the data in the PortletInstance (e.g. ensuring
            // there are ConfigFieldInstances for each ConfigField of the Portlet that this
            // PortletInstance is an Instance of).
            foreach (ConfigFieldInstance configInstance in pi.ConfigFieldInstances) {
                if (configInstance.PortletInstance == null)
                    configInstance.PortletInstance = pi;
            }
            pi.User = user;
            this._db.CreatePortletInstance(pi);
            return Json(new Dictionary<int, int>());
        }
    }
}
