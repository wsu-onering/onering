using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace onering.Controllers {
    [Authorize]
    public class CatalogController : Controller {
        private readonly IConfiguration _configuration;
        public CatalogController(IConfiguration configuration) {
            _configuration = configuration;
        }

        public IActionResult Index() {
            return View();
        }

    }
}