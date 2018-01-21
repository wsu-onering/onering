using System;
namespace onering.Models
{
	/// ToDoItemView represents a superset of the data in the ToDoItems class. The
	/// extra fields are data about the the user and source of this ToDo item.
	public class ToDoItemView
	{
		public string Title { get; set; }
		public string Link { get; set; }
		public bool IsComplete { get; set; }
		public DateTime DueDate { get; set; }
		public int ID {get; set;}
		/// UserID is the ID of the user this ToDo item is associate with.
		public string UserID {get; set;}
		/// SourceID is the ID of the remote data source that this ToDo item came from.
		/// An ID is used instead of the URI for that source so that the backend is
		/// explicitly in charge of where requests are made to.
		/// There is a one-to-one association between a source ID and the URI for that source.
		public string SourceID {get; set;}
	}
}