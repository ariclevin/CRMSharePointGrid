using System;
using System.Runtime.CompilerServices;

namespace SPCAMLQueryBuilder
{
	public class CAMLQueryFilter
	{
        #region Propeeties
        public string query { get; set; }
        public string FieldName { get; set; }
        public FieldType FieldType { get; set; }
        public string FieldValue { get; set; }
        public int FieldIdValue { get; set; }

        #endregion

        public CAMLQueryFilter()
		{
            Markups.InitializeMarkups();
		}

		protected string getQueryTypeMarkUp(QueryType queryType)
		{
			string str;
			try
			{
				string empty = string.Empty;
				switch (queryType)
				{
					case QueryType.Equal:
						empty = Markups.Equal;
						break;
					case QueryType.NotEqual:
						empty = Markups.NotEqual;
						break;
					case QueryType.GreaterThan:
						empty = Markups.GreaterThan;
						break;
					case QueryType.GreaterThanOrEqual:
						empty = Markups.GreaterThanOrEqual;
						break;
					case QueryType.LessThan:
						empty = Markups.LessThan;
						break;
					case QueryType.LessThanOrEqual:
						empty = Markups.LessThanOrEqual;
						break;
					case QueryType.BeginsWith:
						empty = Markups.BeginsWith;
						break;
                    case QueryType.EndsWith:
                        empty = Markups.EndsWith;
                        break;
                    case QueryType.Contains:
						empty = Markups.Contains;
						break;
					case QueryType.DateRangesOverlap:
						empty = Markups.DateRangeOverlaps;
						break;
					default:
                        empty = Markups.Equal;
						break;
				}
				str = empty;
			}
			catch (Exception exception)
			{
				throw exception;
			}
			return str;
		}

		public override string ToString()
		{
			return this.query;
		}
	}

    public class CAMLQueryDateTimeFilter : CAMLQueryFilter
    {
        public CAMLQueryDateTimeFilter(string fieldName, DateTime date, QueryType queryType, bool includeTimeValue = false)
        {
            try
            {
                string str = "<Value IncludeTimeValue=\"{1}\" Type=\"{2}\">{3}</Value>";
                string str1 = string.Format(base.getQueryTypeMarkUp(queryType), Markups.FieldRef, str);
                object[] field = new object[] { fieldName, includeTimeValue.ToString(), "DateTime", date.ToString("yyyy-MM-ddTHH:mm:ssZ") };
                base.query = string.Format(str1, field);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }

    public class CAMLQueryGenericFilter : CAMLQueryFilter
    {
        public CAMLQueryGenericFilter(string fieldName, FieldType fieldType, string fieldValue, QueryType queryType)
        {
            try
            {
                this.FieldName = fieldName;
                this.FieldType = fieldType;
                this.FieldValue = fieldValue;
                string empty = string.Empty;
                empty = string.Format(base.getQueryTypeMarkUp(queryType), Markups.FieldRef, Markups.Value);
                base.query = string.Format(empty, fieldName, fieldType.ToString(), fieldValue);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public CAMLQueryGenericFilter(string fieldName, bool isNull)
        {
            try
            {
                string str = string.Format((isNull ? Markups.IsNull : Markups.IsNotNull), Markups.FieldRef);
                base.query = string.Format(str, fieldName);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }

    public class CAMLQueryLookupFilter : CAMLQueryFilter
    {

        public CAMLQueryLookupFilter(string fieldName, string fieldValue, QueryType queryType)
        {
            try
            {
                this.FieldName = fieldName;
                this.FieldType = FieldType.Lookup;
                this.FieldValue = fieldValue;
                string empty = string.Empty;
                empty = string.Format(base.getQueryTypeMarkUp(queryType), Markups.FieldRef, Markups.Value);
                base.query = string.Format(empty, fieldName, FieldType.Lookup.ToString(), fieldValue);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public CAMLQueryLookupFilter(string fieldName, int fieldValue, QueryType queryType)
        {
            try
            {
                this.FieldName = fieldName;
                this.FieldType = FieldType.Lookup;
                this.FieldIdValue = fieldValue;
                string empty = string.Empty;
                empty = string.Format(base.getQueryTypeMarkUp(queryType), Markups.LookupFieldRef, Markups.Value);
                base.query = string.Format(empty, fieldName, FieldType.Lookup.ToString(), fieldValue);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }



    }
}