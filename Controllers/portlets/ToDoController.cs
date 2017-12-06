using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

using onering.Models;

namespace onering.Controllers
{
    [Authorize]
    public class TodoPortletController : Controller
    {
        // GET: TodoPortlet
        public async Task<IActionResult> Index()
        {
            Debug.WriteLine(HttpContext.User.ToString());
            List<ToDoItem> todos;
            string requestUrl = "http://todotestsite.azurewebsites.net/api/values/";
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // pass
                string responseString = await response.Content.ReadAsStringAsync();
                todos = JsonConvert.DeserializeObject<List<ToDoItem>>(responseString);
            }
            else
            {
                todos = new List<ToDoItem>();
                ViewBag.ErrorMessage = "UnexpectedError";
            }

            return View(todos);
        }
    }
}