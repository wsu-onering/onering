using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;


using onering.Helpers;
using onering.Models;

namespace onering.Controllers
{
    [Authorize]
    public class ToDoPortletController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly IGraphSdkHelper _graphSdkHelper;
        public ToDoPortletController(IConfiguration configuration, IHostingEnvironment hostingEnvironment, IGraphSdkHelper graphSdkHelper)
        {
            _configuration = configuration;
            _env = hostingEnvironment;
            _graphSdkHelper = graphSdkHelper;
        }
        // GET: TodoPortlet
        public async Task<IActionResult> Index()
        {
            Debug.WriteLine(HttpContext.User.ToString());
            string email = User.Identity.Name ?? User.FindFirst("preferred_username").Value;
            // Get user's id for token cache.
            var identifier = User.FindFirst(Startup.ObjectIdentifierType)?.Value;
            // Initialize the GraphServiceClient.
            var graphClient = _graphSdkHelper.GetAuthenticatedClient(identifier);
            Microsoft.Graph.User user = await GraphService.GetUser(graphClient, email, HttpContext);

            Dictionary<string, string> datasources = new Dictionary<string, string>{
                { "1", "http://todotestsite.azurewebsites.net/api/values/" }, 
                { "2", "http://todotestsite2.azurewebsites.net/api/values/" }
            };
            // string requestUrl = "http://todotestsite.azurewebsites.net/api/values/";

            List<ToDoItem> todos = new List<ToDoItem>();
            string id = user.Id;
            foreach (KeyValuePair<string, string> entry in datasources){
                HttpClient client = new HttpClient();
                string sourceID = entry.Key;
                string requestUrl = entry.Value;
                requestUrl += user.Id;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    string responseString = await response.Content.ReadAsStringAsync();
                    List<ToDoItem> newtodos = JsonConvert.DeserializeObject<List<ToDoItem>>(responseString);
                    foreach (ToDoItem item in newtodos){
                       item.Source = entry.Key; 
                        todos.Add(item);
                    }
                }
            }
            Debug.Print("User Id is: {0}", id);

            return View(todos);
        }
    }
}