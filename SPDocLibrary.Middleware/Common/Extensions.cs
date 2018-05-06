using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPDocLibrary.Middleware.Common
{
    public static class Extensions
    {
        public static Guid ToGuid(this string s)
        {
            Guid rc = Guid.Empty;
            bool isGuid = Guid.TryParse(s, out rc);
            return rc;
        }

        public static Uri ToUri(this string s)
        {
            Uri rc;
            bool isCreated = Uri.TryCreate(s, UriKind.Absolute, out rc);

            if (isCreated)
                return rc;
            else
                return null;
        }

        public static int ToInt(this string s)
        {
            int rc = 0;
            bool isInt = int.TryParse(s, out rc);

            if (isInt)
                return rc;
            else
                return 0;
        }

        public static DateTime ToDate(this string s)
        {
            DateTime rc;
            bool isDate = DateTime.TryParse(s, out rc);

            if (isDate)
                return rc;
            else
                return DateTime.MaxValue;
        }

        public static bool? ToBool(this string s)
        {
            bool rc;
            bool isBool = bool.TryParse(s, out rc);

            if (isBool)
                return rc;
            else
                return null;

        }
    }
}