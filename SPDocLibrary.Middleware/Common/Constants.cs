using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SPDocLibrary.Middleware.Common
{
    public static class Constants
    {
        public static class AUTH_HEADER
        {
            public const string BASIC = "basic";
        }
    }

    public static class APISetting
    {
        public static string GetString(string settingName)
        {
            string rc = "";
            if (ConfigurationManager.AppSettings[settingName] != null)
            {
                rc = ConfigurationManager.AppSettings[settingName].ToString();
            }
            return rc;
        }

        public static int GetInt(string settingName)
        {
            int rc = int.MinValue;
            if (ConfigurationManager.AppSettings[settingName] != null)
            {
                string temp = ConfigurationManager.AppSettings[settingName].ToString();
                int tempInt = 0;
                if (int.TryParse(temp, out tempInt))
                    rc = tempInt;
            }
            return rc;
        }

        public static Guid GetGuid(string settingName)
        {
            Guid rc = Guid.Empty;
            if (ConfigurationManager.AppSettings[settingName] != null)
            {
                string temp = ConfigurationManager.AppSettings[settingName].ToString();
                Guid tempGuid = Guid.Empty;
                if (Guid.TryParse(temp, out tempGuid))
                    rc = tempGuid;
            }
            return rc;
        }
    }
}