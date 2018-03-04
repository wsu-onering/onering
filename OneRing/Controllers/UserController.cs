using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using onering.Models;

namespace onering.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;
        private Database.IOneRingDB _db;
        public UserController(IConfiguration configuration, Database.IOneRingDB db) {
            _configuration = configuration;
            _db = db;
        }

        public IActionResult Index() {
            string id = this.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            return Json(this._db.ListOneRingUsers(id), new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}