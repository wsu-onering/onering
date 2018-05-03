using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System;

using Newtonsoft.Json;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

using onering.Helpers;
using onering.Models;
using System.Linq;

namespace onering.Controllers.Portlets
{
    //public class ToDoPortlet : Models.Interfaces.IPortlet
    //{
    //    public string PortletName { get { return _PortletName; } }
    //    public string PortletDescription { get { return _PortletDescription; } }
    //    public string PortletIconPath { get { return _PortletIconPath; } }
    //    public string PortletPath { get { return typeof(ToDoController).Name.Replace("Controller", ""); } }
    //    private static string _PortletName = "ToDo";
    //    private static string _PortletDescription = "View all the items in your To-Do lists.";
    //    private static string _PortletIconPath = "http://lelandbatey.com/favicon.ico";
    //}

    public class ToDoController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly IGraphAuthProvider _graphAuthProvider;
        private Database.IOneRingDB _db;

        public ToDoController(IConfiguration configuration, IHostingEnvironment hostingEnvironment, IGraphAuthProvider graphAuthProvider, Database.IOneRingDB db)
        {
            _configuration = configuration;
            _env = hostingEnvironment;
            _graphAuthProvider = graphAuthProvider;
            _db = db;
        }
        
        [Authorize]
        // GET: TodoPortlet
        public async Task<IActionResult> Index(int id)
        {
            // Get user id
            string userID = this.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            // Get all data sources user configured
            List<ConfigFieldInstance> configInstances = _db.ListConfigFieldInstances(id);
            
            // Retrieve all ToDo items to be sent into view
            List<ToDoItemView> todosItems = new List<ToDoItemView>();
            foreach (ConfigFieldInstance configInstance in configInstances)
            {
                HttpClient client = new HttpClient();
                string sourceID = configInstance.ID.ToString();
                string requestUrl = configInstance.ConfigFieldOption.Value;
                requestUrl += userID;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                string bearerToken = await _graphAuthProvider.GetUserAccessTokenAsync(userID);
                request.Headers.Add("Bearer", bearerToken);
                HttpResponseMessage response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode == true)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    List<ToDoItem> newtodos = JsonConvert.DeserializeObject<List<ToDoItem>>(responseString);
                    foreach (ToDoItem item in newtodos) {
                        ToDoItemView viewItem = new ToDoItemView(item) {
                            SourceID = sourceID,
                            UserID = userID
                        };
                        todosItems.Add(viewItem);
                    }
                }
            }

            return View("~/Views/Portlets/ToDo/Index.cshtml", todosItems);
        }

        //// POST: TodoPortlet
        //[HttpPost]
        //public async Task<IActionResult> MarkDone([FromBody] Dictionary<string, string> data)
        //{
        //    Debug.WriteLine(String.Format("Request data: {0}", JsonConvert.SerializeObject(data, Formatting.Indented)));
        //    string userID = data["userID"];
        //    string sourceID = data["sourceID"];
        //    string itemID = data["itemID"];

        //    // Find the correct remote source URL
        //    string correctSource = "";
        //    if (datasources.ContainsKey(sourceID)) {
        //        correctSource = datasources[sourceID];
        //    } else {
        //        return StatusCode(500);
        //    }
        //    Debug.WriteLine(String.Format("Datasource (no query): {0}", correctSource));
        //    // Populate the remote URL with the correct query parameters
        //    Dictionary<string, string> parameters = new Dictionary<string, string>{
        //        {"guid", userID},
        //        {"todoId", itemID},
        //    };
        //    string fullUrl = QueryHelpers.AddQueryString(correctSource, parameters);
        //    Debug.WriteLine(String.Format("Datasource (with query included): {0}", fullUrl));

        //    // Make the PUT request
        //    HttpClient client = new HttpClient();
        //    HttpResponseMessage resp = await client.PutAsync(fullUrl, null);
        //    // If the response isn't successfull, this will throw an error
        //    resp.EnsureSuccessStatusCode();

        //    return Json(new Dictionary<int, int>());
        //}
    }
}
