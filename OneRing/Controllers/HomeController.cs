using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

using onering.Helpers;
using onering.Models;
using Newtonsoft.Json;

namespace onering.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly IGraphSdkHelper _graphSdkHelper;
        private Database.IOneRingDB _db;

        public HomeController(IConfiguration configuration, IHostingEnvironment hostingEnvironment, IGraphSdkHelper graphSdkHelper, Database.IOneRingDB db)
        {
            _configuration = configuration;
            _env = hostingEnvironment;
            _graphSdkHelper = graphSdkHelper;
            _db = db;
        }

        [Authorize]
        // Load user's profile
        public async Task<IActionResult> Index(string email)
        {
            // We're gonna throw some configs into the page for verification,
            // delete these lines in production
            var azureOptions = new Extensions.AzureAdOptions();
            _configuration.Bind("AzureAd", azureOptions);
            ViewData["Configvals"] = JsonConvert.SerializeObject(azureOptions);
            ViewData["Configvals"] += JsonConvert.SerializeObject(Environment.GetEnvironmentVariables());

            if (User.Identity.IsAuthenticated) {
                // Get users's email
                email = email ?? User.Identity.Name ?? User.FindFirst("preferred_username").Value;
                ViewData["Email"] = email;

                // Get user's id for token cache
                var identifier = User.FindFirst(Startup.ObjectIdentifierType)?.Value;

                // Initialize the GraphServiceClient
                var graphClient = _graphSdkHelper.GetAuthenticatedClient(identifier);
                ViewData["Response"] = await GraphService.GetUserJson(graphClient, email, HttpContext);
                ViewData["Picture"] = await GraphService.GetPictureBase64(graphClient, email, HttpContext);
            }

            // Get user's OneRingUser id
            string id = this.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            // Check if the user exists in the database
            OneRingUser oneRingUser = _db.ListOneRingUsers(id).FirstOrDefault();
            if (oneRingUser == null) {
                // Add new user to database
                oneRingUser = new OneRingUser { GraphID = id };
                _db.CreateOneRingUser(oneRingUser);
                // Return empty list of portlet instances
                return View(new List<PortletInstance>());
            }
            else {
                // Return list of portlets the user has
                List<PortletInstance> portletInstances = _db.ListPortletInstances(oneRingUser);
                return View(portletInstances);
            }
        }
        
        [Authorize]
        [HttpPost]
        // Update portlet instance's position and size
        public void Update(IEnumerable<PortletInstance> portletInstances)
        {
            _db.UpdatePortletInstances(portletInstances);
        }

        //[Authorize]
        //[HttpPost]
        //// Send an email message from the current user.
        //public async Task<IActionResult> SendEmail(string recipients)
        //{
        //    if (string.IsNullOrEmpty(recipients))
        //    {
        //        TempData["Message"] = "Please add a valid email address to the recipients list!";
        //        return RedirectToAction("Index");
        //    }

        //    try
        //    {
        //        // Get user's id for token cache.
        //        var identifier = User.FindFirst(Startup.ObjectIdentifierType)?.Value;

        //        // Initialize the GraphServiceClient.
        //        var graphClient = _graphSdkHelper.GetAuthenticatedClient(identifier);

        //        // Send the email.
        //        await GraphService.SendEmail(graphClient, _env, recipients, HttpContext);

        //        // Reset the current user's email address and the status to display when the page reloads.
        //        TempData["Message"] = "Success! Your mail was sent.";
        //        return RedirectToAction("Index");
        //    }
        //    catch (ServiceException se)
        //    {
        //        Debug.Print("Error occurred while attempting to send email: {0}, {1}", se, se.Error.Code);
        //        if (se.Error.Code == "Caller needs to authenticate.") return new EmptyResult();
        //        return RedirectToAction("Error", "Home", new { message = "Error: " + se.Error.Message });
        //    }
        //}

        //[AllowAnonymous]
        //public IActionResult About()
        //{
        //    ViewData["Message"] = "Your application description page.";

        //    return View();
        //}

        //[AllowAnonymous]
        //public IActionResult Contact()
        //{
        //    ViewData["Message"] = "Your contact page.";

        //    return View();
        //}

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
