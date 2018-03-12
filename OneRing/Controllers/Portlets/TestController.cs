using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using onering.Models;

namespace onering.Controllers.Portlets
{
    public class TestPortlet : Models.Interfaces.IPortlet {
        public string PortletName {get { return _PortletName; } }
        public string PortletDescription { get { return _PortletDescription; } }
        public string PortletIconPath { get { return _PortletIconPath; } }
        public string PortletPath  { get { return typeof(ToDoController).Name.Replace("Controller", ""); } }
        private static string _PortletName = "WarpSpeed";
        private static string _PortletDescription = "Travel the galaxy at warp factor 5!";
        private static string _PortletIconPath = "http://lorempixel.com/200/200/city/";
    }

    public class TestController : Controller
    {
        private readonly IConfiguration _configuration;
        private Database.IOneRingDB _db;

        public TestController(IConfiguration configuration, Database.IOneRingDB db)
        {
            _configuration = configuration;
            _db = db;
        }

        public IActionResult Index(int id)
        {
            return View("~/Views/Portlets/TestPortlet/Index.cshtml");
        }
    }
}