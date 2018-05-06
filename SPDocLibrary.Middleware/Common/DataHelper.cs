using SPDocLibrary.Middleware.Models;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace SPDocLibrary.Middleware.Common
{
    public static class DataHelper
    {
        public static SharePointDocument ListItemToCustomerDocument(ListItem item)
        {
            SharePointDocument file = new SharePointDocument();
            file.FileId = (item["UniqueId"].ToString().ToGuid());
            file.MasterId = FieldValueToString(item["MasterId"]);
            file.MasterNumber = FieldValueToString(item["MasterId"]);
            file.MasterName = FieldValueToString(item["MasterId"]);
            file.DocumentType = FieldLookupValueToLookupItem(item["DocumentType"]);
            file.FileName = item["FileLeafRef"].ToString();
            file.FilePath = item["FileRef"].ToString();

            return file;
        }


        public static LookupItem FieldLookupValueToLookupItem(object field)
        {
            LookupItem rc = null;
            if (field != null)
            {
                if (field is FieldLookupValue)
                {
                    FieldLookupValue lookupField = (FieldLookupValue)field;
                    rc = new LookupItem(lookupField.LookupId, lookupField.LookupValue);
                }
                else
                    rc = new LookupItem(0, string.Empty);
            }
            else
                rc = new LookupItem(0, string.Empty);

            return rc;
        }

        public static string FieldValueToString(object field)
        {
            string rc = "";
            if (field != null)
            {
                rc = field.ToString();
            }
            return rc;
        }

        public static DateTime FieldValueToDate(object field)
        {
            DateTime rc = DateTime.MaxValue;
            if (field != null)
            {
                bool isDate = DateTime.TryParse(field.ToString(), out rc);
                if (isDate)
                    return rc;
                else
                    return DateTime.MaxValue;
            }
            else
                return rc;
        }

        public static bool ValidateUploadFields(SharePointDocument doc)
        {
            bool rc = true;
            if (string.IsNullOrEmpty(doc.MasterId))
            {
                rc = false;
            }
            if (string.IsNullOrEmpty(doc.MasterName))
            {
                rc = false;
            }
            return rc;
        }

        public static LookupItem StringToLookupItem(string listName, string itemTitle)
        {
            if (itemTitle.Contains(":"))
            {
                string[] itemTitleSplit = itemTitle.Split(':');
                LookupItem rc = new LookupItem(itemTitleSplit[0].ToInt(), itemTitleSplit[1]);
                if (rc.ItemId == 0)
                    return null;
                else
                    return rc;
            }
            else
            {
                int? itemId = SPHelper.GetListItemId(listName, itemTitle);
                if (itemId.HasValue)
                {
                    return new LookupItem(itemId.Value, itemTitle);
                }
                else
                {
                    return null;
                }
            }
        }

        public static string DecodeAuthorizationString(System.Net.Http.Headers.AuthenticationHeaderValue authorizationHeader)
        {
            string userPass = string.Empty;
            if (authorizationHeader == null)
            {
                // throw HttpResponseHelper.GetUnauthorizedResponseException("Auth Header is null!");
                string emailAddress = ConfigurationManager.AppSettings["Email"].ToString();
                string password = ConfigurationManager.AppSettings["Password"].ToString();
                userPass = string.Format("{0}:{1}", emailAddress, password);
            }
            else
            {
                var authHeader = authorizationHeader;
                if (authHeader.Scheme.ToLower() != Constants.AUTH_HEADER.BASIC)
                {
                    throw HttpResponseHelper.GetUnauthorizedResponseException("Auth Header is not using BASIC scheme!");
                }
                var encodedUserPass = authHeader.Parameter;
                userPass = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUserPass));
            }
            return userPass;
        }

    }
}