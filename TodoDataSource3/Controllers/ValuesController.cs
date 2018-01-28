using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TodoDataSource1.Models;

namespace TodoDataSource3.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        //////////////////////////////////////////////////////////////////////////////////////
        // Constructor
        //////////////////////////////////////////////////////////////////////////////////////
        public ValuesController()
        {
        }


        //////////////////////////////////////////////////////////////////////////////////////
        // Methods
        //////////////////////////////////////////////////////////////////////////////////////
        // GET api/values/####
        public IEnumerable<TodoItemModel> Get(string guid)
        {
            UserDatabase userDB = new UserDatabase();

            // Open database
            userDB.Open();
            // Get todo items with name
            IEnumerable<TodoItemModel> todoItems = userDB.GetTodoItems(guid);
            // Close database
            userDB.Close();

            return todoItems;
        }

        // PUT api/values?guid=####&todId=#
        public void Put([FromUri]string guid, [FromUri]int todoId)
        {
            UserDatabase userDB = new UserDatabase();

            // Open database
            userDB.Open();
            // Update complete todo item
            userDB.CompleteTodoItem(guid, todoId);
            // Close database
            userDB.Close();
        }
    }
}
