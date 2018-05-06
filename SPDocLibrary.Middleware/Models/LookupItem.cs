using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPDocLibrary.Middleware.Models
{
    public class LookupItem
    {
        public LookupItem(int listItemId, string title, KeyValuePair<int, string> parent)
        {
            ItemId = listItemId;
            Title = title;
            Parent = parent;
        }

        public LookupItem(int listItemId, string title)
        {
            ItemId = listItemId;
            Title = title;
        }

        public int ItemId { get; set; }

        public string Title { get; set; }

        public KeyValuePair<int, string> Parent { get; set; }
    }
}