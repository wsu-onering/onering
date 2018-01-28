using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using TodoDataSource1.Models;

namespace TodoDataSource1.Controllers
{
    [Authorize]
    public class TodoController : ApiController
    {
        //////////////////////////////////////////////////////////////////////////////////////
        // Properties
        //////////////////////////////////////////////////////////////////////////////////////



        //////////////////////////////////////////////////////////////////////////////////////
        // Constructor
        //////////////////////////////////////////////////////////////////////////////////////
        public TodoController()
        {
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Methods
        //////////////////////////////////////////////////////////////////////////////////////
        // GET api/<controller>/5
        public IEnumerable<TodoItemModel> Get(string guid)
        {
            Claim claim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            if (claim.Value != "user_impersonation")
                throw new HttpResponseException(
                    new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found" }
                );
            
            // Open the database
            UserDatabase userDB = new UserDatabase();
            userDB.Open();

            // Get todo items with name
            IEnumerable<TodoItemModel> todoItems = userDB.GetTodoItems(guid);

            // Close database
            userDB.Close();

            return todoItems;
        }

        // PUT api/<controller>/5
        public void Put([FromUri]string guid, [FromUri]int todoId)
        {
            Claim claim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            if (claim.Value != "user_impersonation")
                throw new HttpResponseException(
                    new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found" }
                );

            // Open database
            UserDatabase userDB = new UserDatabase();
            userDB.Open();

            // Update complete todo item
            userDB.CompleteTodoItem(guid, todoId);

            // Close database
            userDB.Close();
        }
    }
}