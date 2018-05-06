using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPCAMLQueryBuilder
{
    public static class Markups
    {
        #region Properties

        public static string Equal { get; private set; }
        public static string NotEqual { get; private set; }
        public static string GreaterThan { get; private set; }
        public static string GreaterThanOrEqual { get; private set; }
        public static string LessThan { get; private set; }
        public static string LessThanOrEqual { get; private set; }
        public static string BeginsWith { get; private set; }
        public static string EndsWith { get; private set; }
        public static string Contains { get; private set; }
        public static string DateRangeOverlaps { get; private set; }
        public static string IsNull { get; private set; }
        public static string IsNotNull { get; private set; }
        public static string FieldRef { get; private set; }
        public static string LookupFieldRef { get; private set; }
        public static string Value { get; private set; }
        public static string Where { get; private set; }
        public static string OrderBy { get; private set; }
        public static string OrderByFieldValue { get; private set; }
        public static string And { get; private set; }
        public static string Or { get; private set; }
        public static string ViewFieldRef { get; private set; }
        public static string ViewFields { get; private set; }
        public static string ViewScope { get; private set; }
        public static string ViewScopeRecursive { get; private set; }
        #endregion

        public static void InitializeMarkups()
        {
            Equal = "<Eq>{0}{1}</Eq>";
            NotEqual = "<Neq>{0}{1}</Neq>";
            GreaterThan = "<Gt>{0}{1}</Gt>";
            GreaterThanOrEqual = "<Geq>{0}{1}</Geq>";
            LessThan = "<Lt>{0}{1}</Lt>";
            LessThanOrEqual = "<Leq>{0}{1}</Leq>";
            BeginsWith = "<BeginsWith>{0}{1}</BeginsWith>";
            BeginsWith = "<EndsWith>{0}{1}</EndsWith>";
            Contains = "<Contains>{0}{1}</Contains>";
            DateRangeOverlaps= "<DateRangesOverlap>{0}{1}</DateRangesOverlap>";
            IsNull= "<IsNull>{0}</IsNull>";
            IsNotNull = "<IsNotNull>{0}</IsNotNull>";
            FieldRef = "<FieldRef Name=\"{0}\" />";
            LookupFieldRef = "<FieldRef Name=\"{0}\" LookupId=\"True\" />";
            Value = "<Value Type=\"{1}\">{2}</Value>";
            Where = "<Where>{0}</Where>";
            OrderBy = "<OrderBy>{0}</OrderBy>";
            And = "<And>{0}{1}</And>";
            Or = "<Or>{0}{1}</Or>";
            OrderByFieldValue = "<FieldRef Name=\"{0}\" Ascending=\"{1}\" />";
            ViewFieldRef = "<FieldRef Name=\"{0}\" />";
            ViewFields = "<ViewFields>{0}</ViewFields>";
            ViewScope = "<View><Query>{0}</Query>{1}</View>";
            ViewScopeRecursive = "<View Scope='RecursiveAll'><Query>{0}</Query>{1}</View>";
        }


    }
}
