﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TodoDataSource2.Models
{
    [DataContract]
    public class TodoItemModel
    {
        //////////////////////////////////////////////////////////////////////////////////////
        // Properties
        //////////////////////////////////////////////////////////////////////////////////////
        [DataMember(Name = "ID")]
        private int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        [DataMember(Name = "Title")]
        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        [DataMember(Name = "Link")]
        private string link;
        public string Link
        {
            get { return link; }
            set { link = value; }
        }

        [DataMember(Name = "DueDate")]
        private DateTime dueDate;
        public DateTime DueDate
        {
            get { return dueDate; }
            set { dueDate = value; }
        }

        private DateTime completeDate;
        public DateTime CompleteDate
        {
            get { return completeDate; }
            set { completeDate = value; }
        }

        private bool isComplete;
        public bool IsComplete
        {
            get { return isComplete; }
            set { isComplete = value; }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Constructor
        //////////////////////////////////////////////////////////////////////////////////////
        public TodoItemModel()
        {
        }

        public TodoItemModel(string title, string link, DateTime dueDate)
        {
            this.title = title;
            this.link = link;
            this.dueDate = dueDate;
        }


        //////////////////////////////////////////////////////////////////////////////////////
        // Methods
        //////////////////////////////////////////////////////////////////////////////////////

    }
}