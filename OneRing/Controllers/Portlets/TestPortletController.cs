using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using onering.Helpers;

namespace onering.Controllers.Portlets
{
    public class TestPortletController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly IGraphAuthProvider _graphAuthProvider;


        public TestPortletController(IConfiguration configuration, IHostingEnvironment hostingEnvironment, IGraphAuthProvider graphAuthProvider)
        {
            _configuration = configuration;
            _env = hostingEnvironment;
            _graphAuthProvider = graphAuthProvider;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}