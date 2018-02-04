using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using System;

using Newtonsoft.Json;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;


using onering.Helpers;
using onering.Models;

namespace onering.Controllers
{
    public class ToDoPortletController : Controller
    {
        private readonly Dictionary<string, string> datasources = new Dictionary<string, string>{
                // { "1", "http://todotestsite.azurewebsites.net/api/values/" },
                // { "2", "http://todotestsite2.azurewebsites.net/api/values/" }
                { "1", "http://localhost:5555/" },
            };
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly IGraphAuthProvider _graphAuthProvider;
        public ToDoPortletController(IConfiguration configuration, IHostingEnvironment hostingEnvironment, IGraphAuthProvider graphAuthProvider)
        {
            _configuration = configuration;
            _env = hostingEnvironment;
            _graphAuthProvider = graphAuthProvider;
        }
        [Authorize]
        // GET: TodoPortlet
        public async Task<IActionResult> Index()
        {
            string id = this.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            Debug.Print("User Id is: {0}", id);

            List<ToDoItemView> todos = new List<ToDoItemView>();
            foreach (KeyValuePair<string, string> entry in datasources){
                HttpClient client = new HttpClient();
                string sourceID = entry.Key;
                string requestUrl = entry.Value;
                requestUrl += id;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                string bearerToken = await _graphAuthProvider.GetUserAccessTokenAsync(id);
                request.Headers.Add("Bearer", bearerToken);
                HttpResponseMessage response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    string responseString = await response.Content.ReadAsStringAsync();
                    List<ToDoItem> newtodos = JsonConvert.DeserializeObject<List<ToDoItem>>(responseString);
                    foreach (ToDoItem item in newtodos){
                       ToDoItemView viewItem = new ToDoItemView();
                       viewItem.Title = item.Title;
                       viewItem.Link = item.Link;
                       viewItem.IsComplete = item.IsComplete;
                       viewItem.DueDate = DateTime.Parse(item.DueDate);
                       viewItem.ID = item.ID;
                       viewItem.SourceID = entry.Key;
                       viewItem.UserID = id;
                       todos.Add(viewItem);
                    }
                }
            }
            Debug.Print("User Id is: {0}", id);

            return View(todos);
        }
        // POST: TodoPortlet
        [HttpPost]
        public async Task<IActionResult> MarkDone([FromBody] Dictionary<string, string> data){
            Debug.WriteLine(String.Format("Request data: {0}", JsonConvert.SerializeObject(data, Formatting.Indented)));
            string userID = data["userID"];
            string sourceID = data["sourceID"];
            string itemID = data["itemID"];

            // Find the correct remote source URL
            string correctSource = "";
            if (datasources.ContainsKey(sourceID)) {
                correctSource = datasources[sourceID];
            } else {
                return StatusCode(500);
            }
            Debug.WriteLine(String.Format("Datasource (no query): {0}", correctSource));
            // Populate the remote URL with the correct query parameters
            Dictionary<string, string> parameters = new Dictionary<string, string>{
                {"guid", userID},
                {"todoId", itemID},
            };
            string fullUrl = QueryHelpers.AddQueryString(correctSource, parameters);
            Debug.WriteLine(String.Format("Datasource (with query included): {0}", fullUrl));

            // Make the PUT request
            HttpClient client = new HttpClient();
            HttpResponseMessage resp = await client.PutAsync(fullUrl, null);
            // If the response isn't successfull, this will throw an error
            resp.EnsureSuccessStatusCode();

            return Json(new Dictionary<int, int>());
        }
    }
}