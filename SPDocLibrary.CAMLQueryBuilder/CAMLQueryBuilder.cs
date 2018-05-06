using System;
using System.Collections.Generic;

namespace SPCAMLQueryBuilder
{
	public class CAMLQueryBuilder
	{
        string Query { get; set; }
        string ViewFields { get; set; }
        bool IsBuildComplete { get; set; }
        bool IsRecursive { get; set; }

		public CAMLQueryBuilder(CAMLQueryFilter filter)
		{
            Markups.InitializeMarkups();

            try
			{
				if (string.IsNullOrEmpty(filter.ToString()))
				{
					throw new Exception("Filter passed does not contain any values");
				}
				Query = filter.ToString();
				IsBuildComplete = false;
			}
			catch (Exception exception)
			{
				throw exception;
			}
		}

		public void ANDFilter(CAMLQueryFilter filter)
		{
			try
			{
				if (IsBuildComplete)
				{
					throw new Exception("Cannot AND after Build() method is called");
				}
				Query = string.Format(Markups.And, filter.ToString(), Query);
			}
			catch (Exception exception)
			{
				throw exception;
			}
		}

        public void DocumentFilter(FSObjType objType, bool isRecursive)
        {
            if (objType == FSObjType.Document)
            {
                CAMLQueryGenericFilter filterDocs = new CAMLQueryGenericFilter("FSObjType", FieldType.Integer, FSObjType.Document.ToString(), QueryType.Equal);
                ANDFilter(filterDocs);
            }
            else if (objType == FSObjType.Folder)
            {
                CAMLQueryGenericFilter filterFolders = new CAMLQueryGenericFilter("FSObjType", FieldType.Integer, FSObjType.Folder.ToString(), QueryType.Equal);
                ANDFilter(filterFolders);
            }
            IsRecursive = isRecursive;
        }

        public void AddViewField(string fieldName)
        {
            if (!string.IsNullOrEmpty(fieldName))
            {
                ViewFields += string.Format(Markups.ViewFieldRef, fieldName);
            }
        }

        public void AddViewFields(List<string> list)
        {
            if (list.Count > 0)
            {
                foreach (string s in list)
                {
                    ViewFields += string.Format(Markups.ViewFieldRef, s);
                }
            }
        }

		public void BuildQuery()
		{
			try
			{
				if (!IsBuildComplete)
				{
					Query = string.Format(Markups.Where, Query);
				}
				IsBuildComplete = true;
			}
			catch (Exception exception)
			{
				throw exception;
			}
		}

        public void BuildViewFields()
        {
            this.ViewFields = string.Format(Markups.ViewFields, ViewFields);
            if (IsRecursive)
            {
                Query = string.Format(Markups.ViewScopeRecursive, Query, ViewFields);
            }
            else
            {
                Query = string.Format(Markups.ViewScope, Query, ViewFields);
            }
        }



        public CAMLQueryFilter RetrieveFilter()
		{
			CAMLQueryFilter camlQueryFilter;
			try
			{
				if (IsBuildComplete)
				{
                    camlQueryFilter = null;
				}
				else
				{
					CAMLQueryFilter camlQueryFilter1 = new CAMLQueryFilter()
					{
						query = Query.ToString()
					};
                    camlQueryFilter = camlQueryFilter1;
				}
			}
			catch (Exception exception)
			{
				throw exception;
			}
			return camlQueryFilter;
		}

		public void OrderBy(string fieldName, bool isAscending)
		{
			try
			{
				OrderBy(new Dictionary<string, bool>()
				{
					{ fieldName, isAscending }
				});
			}
			catch (Exception exception)
			{
				throw exception;
			}
		}

		public void OrderBy(Dictionary<string, bool> fields)
		{
			try
			{
				if (!IsBuildComplete)
				{
					throw new Exception("Call Build() method before adding OrderBy element to the query");
				}
				this.RemoveElement("OrderBy");
				string empty = string.Empty;
				foreach (KeyValuePair<string, bool> field in fields)
				{
					string _queryORDERBYFieldValueMarkUp = Markups.OrderByFieldValue;
					string key = field.Key;
					bool value = field.Value;
					empty = string.Concat(empty, string.Format(_queryORDERBYFieldValueMarkUp, key, value.ToString()));
				}
				Query = string.Concat(Query, string.Format(Markups.OrderBy, empty));
			}
			catch (Exception exception)
			{
				throw exception;
			}
		}

		public void ORFilter(CAMLQueryFilter filter)
		{
			try
			{
				if (IsBuildComplete)
				{
					throw new Exception("Cannot OR after Build() method is called");
				}
				Query = string.Format(Markups.Or, filter.ToString(), Query);
			}
			catch (Exception exception)
			{
				throw exception;
			}
		}

		public void RemoveElement(string elementName)
		{
			try
			{
				if ((!Query.ToLower().Contains("<" + elementName + ">".ToLower()) ? false : Query.ToLower().Contains("</" + elementName + ">".ToLower())))
				{
					int startPos = Query.ToLower().IndexOf("<" + elementName + ">".ToLower());
					int endPos = Query.ToLower().IndexOf("</" + elementName + ">".ToLower()) + ("</" + elementName + ">").Length + 1;
					Query = Query.Remove(startPos, endPos - startPos);
				}
			}
			catch (Exception exception)
			{
				throw exception;
			}
		}

		public override string ToString()
		{
			string myQuery;
			try
			{
				if (!IsBuildComplete)
				{
					throw new Exception("Call Build() method before using the query");
				}
				myQuery = Query;
			}
			catch (Exception exception)
			{
				throw exception;
			}
			return myQuery;
		}
	}
}