using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using SPDocLibrary.Middleware.Models;
using SPDocLibrary.Middleware.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SPDocLibrary.Middleware.Controllers
{
    [EnableCors(origins: "*", headers: "Content-Type, Authorization", methods: "GET, POST, PUT, OPTIONS, DELETE")]
    public class DocumentsLibraryController : ApiController
    {
        [HttpGet]
        [Route("api/Library/GetByMasterId/{id}")]
        public HttpResponseMessage GetByMasterId([FromUri] string id)
        {
            string authorizationString = DecodeAuthorizationString();
            SPHelper.SetSharePointCredentials(authorizationString);

            List<SharePointDocument> files = new List<SharePointDocument>();

            ListItemCollection list = SPHelper.GetDocumentsById(id);
            if (list != null && list.AreItemsAvailable)
            {
                foreach (ListItem item in list)
                {
                    SharePointDocument file = ListItemToSharePointDocument(item);
                    files.Add(file);
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(files), Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        [Route("api/Library/GetByMasterNumber/{id}")]
        public HttpResponseMessage GetByMasterNumber([FromUri] string id)
        {
            string authorizationString = DecodeAuthorizationString();
            SPHelper.SetSharePointCredentials(authorizationString);

            List<SharePointDocument> files = new List<SharePointDocument>();

            ListItemCollection list = SPHelper.GetDocumentsByNumber(id);
            if (list != null && list.AreItemsAvailable)
            {
                foreach (ListItem item in list)
                {
                    SharePointDocument file = ListItemToSharePointDocument(item);
                    files.Add(file);
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(files), Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        [Route("api/Library/GetByAlternateField")]
        public HttpResponseMessage GetByAlternateField()
        {
            string authorizationString = DecodeAuthorizationString();
            SPHelper.SetSharePointCredentials(authorizationString);

            List<SharePointDocument> files = new List<SharePointDocument>();
            if (!string.IsNullOrEmpty(Request.RequestUri.Query))
            {
                List<KeyValuePair<string, string>> kvpList = Request.GetQueryNameValuePairs().ToList<KeyValuePair<string, string>>();

                string alternateFieldName = "", alternateFieldValue = "";
                foreach (KeyValuePair<string, string> kvp in kvpList)
                {
                    string key = kvp.Key;
                    string val = kvp.Value;

                    switch (key)
                    {
                        case "AlternateFieldName":
                            alternateFieldName = val;
                            break;
                        case "AlternateFieldValue":
                            alternateFieldValue = val;
                            break;
                        default:
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(alternateFieldName) && (!string.IsNullOrEmpty(alternateFieldValue)))
                {
                    ListItemCollection list = SPHelper.GetDocuments(alternateFieldName, alternateFieldValue);
                    if (list != null && list.AreItemsAvailable)
                    {
                        foreach (ListItem item in list)
                        {
                            SharePointDocument file = ListItemToSharePointDocument(item);
                            files.Add(file);
                        }
                    }
                }
            }
            else
            {
                // No Query String Included
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(files), Encoding.UTF8, "application/json");
            return response;
        }

        /*
        [HttpGet]
        [Route("api/Library/Search")]
        public HttpResponseMessage Search()
        {
            string authorizationString = DecodeAuthorizationString();
            SPHelper.SetSharePointCredentials(authorizationString);

            List<SharePointDocument> files = new List<SharePointDocument>();
            if (!string.IsNullOrEmpty(Request.RequestUri.Query))
            {
                List<KeyValuePair<string, string>> kvpList = Request.GetQueryNameValuePairs().ToList<KeyValuePair<string, string>>();

                if (kvpList.Count > 0)
                {
                    ListItemCollection list = SPHelper.GetDocuments(kvpList);
                    if (list.AreItemsAvailable)
                    {
                        foreach (ListItem item in list)
                        {
                            SharePointDocument file = DataHelper.ListItemToCustomerDocument(item);
                            files.Add(file);
                        }
                    }
                }
            }
            else
            {
                // No Query String Included
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(files), Encoding.UTF8, "application/json");
            return response;
        }
        */

        [HttpGet]
        [Route("api/Library/DownloadFile/{id}")]
        public HttpResponseMessage DownloadFile([FromUri] string id)
        {
            string authorizationString = DecodeAuthorizationString();
            SPHelper.SetSharePointCredentials(authorizationString);

            ListItem item = SPHelper.FindFile(id.ToGuid());
            
            string fileName = item["FileLeafRef"].ToString();
            string contentType = System.Web.MimeMapping.GetMimeMapping(fileName);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            Byte[] bytes = SPHelper.DownloadFile(item);
            response.Content = new ByteArrayContent(bytes);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            response.Content.Headers.ContentDisposition.FileName = fileName;
            
            return response;
        }

        [HttpPost]
        [Route("api/Library/UploadDocument")]
        public async Task<HttpResponseMessage> UploadDocument()
        {
            string authorizationString = DecodeAuthorizationString();
            SPHelper.SetSharePointCredentials(authorizationString);

            Guid fileId = Guid.Empty;
            if (!Request.Content.IsMimeMultipartContent())
            {
                HttpResponseException ex = new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                throw ex;
            }

            SharePointDocument doc = new SharePointDocument();

            string targetFolderName = "";
            if (!string.IsNullOrEmpty(Request.RequestUri.Query))
            {
                List<KeyValuePair<string, string>> list = Request.GetQueryNameValuePairs().ToList<KeyValuePair<string, string>>();
                foreach (KeyValuePair<string, string> kvp in list)
                {
                    string key = kvp.Key;
                    string val = kvp.Value;
                    switch (key)
                    {
                        case "MasterId":
                            doc.MasterId = val; //(val == "!" ? "" : val);
                            break;
                        case "DocumentType":
                            doc.DocumentType = StringToLookupItem("DocumentType", val);
                            break;
                        case "MasterNumber":
                            doc.MasterNumber = val;
                            break;
                        case "MasterName":
                            doc.MasterName = val;
                            break;
                        case "EntityName":
                            targetFolderName = SetEntityName(val);
                            break;
                        default:
                            break;
                    }
                    
                }
            }

            if (!ValidateUploadFields(doc))
            {
                HttpResponseException ex = new HttpResponseException(HttpStatusCode.NotAcceptable);
                throw ex;

            }

            MultipartMemoryStreamProvider provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            foreach (var file in provider.Contents)
            {
                var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
                byte[] buffer = await file.ReadAsByteArrayAsync();

                int maxItems = APISetting.GetInt("MAX_ITEMS_PER_FOLDER");
                if (maxItems == int.MinValue)
                    maxItems = 5000;

                if (!string.IsNullOrEmpty(doc.MasterId))
                {
                    ListItem msaFolder = SPHelper.FindFolder(doc.MasterId);
                    if (msaFolder == null)
                    {
                        ListItem rootFolder = SPHelper.GetRootFolderForUpload(targetFolderName, true);
                        string rootFolderUrl = rootFolder["FileRef"].ToString();
                        int childFolders = SPHelper.GetChildFoldersCount(rootFolderUrl);
                        if (childFolders < maxItems)
                        {
                            msaFolder = SPHelper.CreateFolder(rootFolder, rootFolderUrl, doc.MasterId);
                            targetFolderName = doc.MasterId;
                        }
                        else
                        {
                            // CREATE NEXT PARENT FOLDER
                            int currentIndex = rootFolderUrl.Substring(rootFolderUrl.LastIndexOf('_') + 1).ToInt();
                            string newRootFolderName = APISetting.GetString("CUSTOMER_FOLDER_PREFIX") + (++currentIndex).ToString();
                            SPHelper.CreateRootFolder(newRootFolderName);
                            rootFolder = SPHelper.GetRootFolderForUpload(targetFolderName, true);
                            rootFolderUrl = rootFolder["FileRef"].ToString();
                            msaFolder = SPHelper.CreateFolder(rootFolder, rootFolderUrl, doc.MasterId);
                            targetFolderName = doc.MasterId;
                        }

                    }
                    else
                        targetFolderName = doc.MasterId;
                }

                File uploadedFile = SPHelper.UploadFile(fileName, targetFolderName, buffer);
                Common.SPHelper.AddRequiredDocumentProperties(uploadedFile, doc.MasterId, doc.MasterNumber, doc.MasterName, doc.DocumentType);
                fileId = new Guid(uploadedFile.ListItemAllFields["UniqueId"].ToString());
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, new KeyValuePair<string, string>("FileId", fileId.ToString()));
            return response;
        }

        [HttpPost]
        [Route("api/Library/SetFileAttributes")]
        public async Task<HttpResponseMessage> SetFileAttributes()
        {
            if (!Request.Content.IsFormData())
            {
                return new HttpResponseMessage(HttpStatusCode.UnsupportedMediaType);
                // HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.UnsupportedMediaType);
            }

            string authorizationString = DecodeAuthorizationString();
            SPHelper.SetSharePointCredentials(authorizationString);
            NameValueCollection formData = await Request.Content.ReadAsFormDataAsync();
            
            if (formData.Keys.Count > 0)
            {
                SharePointDocument doc = new SharePointDocument();
                doc.FileId = formData.Get("FileId").ToGuid();
                doc.MasterId = formData.Get("MasterId");
                doc.MasterNumber = formData.Get("MasterNumber");
                doc.DocumentType = StringToLookupItem("DocumentType", formData.Get("DocumentType"));
                doc.MasterName = formData.Get("MasterName");

                ListItem file = SPHelper.FindFile(doc.FileId);
                SPHelper.AddDocumentProperties(file, doc);

            }
            return new HttpResponseMessage(HttpStatusCode.OK);

        }

        // DELETE: api/Library/DeleteFile/Guid
        [HttpDelete]
        [Route("api/Library/DeleteFile/{id}")]
        public HttpResponseMessage DeleteFile(string id)
        {
            string authorizationString = DecodeAuthorizationString();
            SPHelper.SetSharePointCredentials(authorizationString);

            ListItem item = SPHelper.FindFile(id.ToGuid());
            SPHelper.DeleteFile(item);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        #region OPTIONS methods

        [HttpOptions]
        [Route("api/CustomerDocuments/GetByMSA/{id}")]
        public HttpResponseMessage GetByMSAOptions([FromUri] string id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpOptions]
        [Route("api/CustomerDocuments/GetByAlternateId")]
        public HttpResponseMessage GetByAlternateIdOptions()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpOptions]
        [Route("api/CustomerDocuments/DownloadFile/{id}")]
        public HttpResponseMessage DownloadFileOptions([FromUri] string id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpOptions]
        [Route("api/CustomerDocuments/UploadDocument")]
        public HttpResponseMessage UploadDocumentOptions()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpOptions]
        [Route("api/CustomerDocuments/SetFileAttributes")]
        public HttpResponseMessage SetFileAttributesOptions()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpOptions]
        [Route("api/CustomerDocuments/DeleteFile/{id}")]
        public HttpResponseMessage DeleteFileOptions(string id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        #endregion

        #region Folder Methods

        // GET: api/Library/Get
        [HttpGet]
        [Route("api/Library/GetRootFolders")]
        public HttpResponseMessage GetRootFolders()
        {
            List<SharePointFolder> spFolders = new List<SharePointFolder>();

            string authorizationString = DecodeAuthorizationString();
            SPHelper.SetSharePointCredentials(authorizationString);
            ListItemCollection folders = SPHelper.GetFolders();
            if (folders != null && folders.AreItemsAvailable)
            {
                foreach (ListItem folder in folders)
                {
                    string folderName = folder["FileLeafRef"].ToString();
                    DateTime createdOn = Convert.ToDateTime(folder["Created_x0020_Date"]);
                    string folderPath = folder["FileRef"].ToString();
                    int totalFiles = Common.SPHelper.GetChildFoldersCount(folderPath);
                    spFolders.Add(new SharePointFolder(folderName, folderPath, totalFiles, createdOn));
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(spFolders), Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        [Route("api/Library/GetFolder/{id}")]
        public HttpResponseMessage Get([FromUri] string id)
        {
            string authorizationString = DecodeAuthorizationString();
            SPHelper.SetSharePointCredentials(authorizationString);
            ListItem folder = Common.SPHelper.FindFolder(id);
            if (folder != null)
            {
                DateTime createdOn = Convert.ToDateTime(folder["Created_x0020_Date"]);
                string folderPath = folder["FileRef"].ToString();
                int totalFiles = Common.SPHelper.GetChildDocumentCount(folderPath);

                SharePointFolder rc = new SharePointFolder(id, folderPath, totalFiles, createdOn);
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(JsonConvert.SerializeObject(rc), Encoding.UTF8, "application/json");
                return response;

            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // POST: api/CustomerFolders
        // public void Post([FromBody]string value)
        // {
        //     Common.SPHelper.CreateRootFolder(value);
        // }

        [HttpPost]
        [Route("api/Library/CreateRootFolder")]
        public async Task<HttpResponseMessage> CreateRootFolder()
        {
            string authorizationString = DecodeAuthorizationString();
            SPHelper.SetSharePointCredentials(authorizationString);

            string body = await Request.Content.ReadAsStringAsync();
            dynamic bodyInfo = JsonConvert.DeserializeObject(body);
            string folderName = bodyInfo.FileName;
            SPHelper.CreateRootFolder(folderName);
            return new HttpResponseMessage(HttpStatusCode.OK);

        }
        #endregion

        private LookupItem StringToLookupItem(string listName, string itemTitle)
        {
            if (itemTitle.Contains(":"))
            {
                string[] itemTitleSplit = itemTitle.Split(':');
                int itemId = 0;
                bool isInt = int.TryParse(itemTitleSplit[0], out itemId);
                if (isInt)
                {
                    // Has List Id in String
                    LookupItem rc = new LookupItem(itemTitleSplit[0].ToInt(), itemTitleSplit[1]);
                    return rc;
                }
                else
                {
                    Guid itemGuid = Guid.Empty;
                    bool isGuid = Guid.TryParse(itemTitleSplit[0], out itemGuid);
                    if (isGuid)
                    {
                        int? listId = SPHelper.GetListItemId(listName, "UniqueItemId", itemGuid.ToString());
                        if (listId.HasValue)
                        {
                            return new LookupItem(listId.Value, itemTitleSplit[1].ToString());
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
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

        private SharePointDocument ListItemToSharePointDocument(ListItem item)
        {
            SharePointDocument file = new SharePointDocument();
            file.FileId = (item["UniqueId"].ToString().ToGuid());
            file.MasterId = item["MasterId"] != null ? item["MasterId"].ToString() : "";
            file.MasterNumber = item["MasterNumber"] != null ? item["MasterNumber"].ToString() : "";
            file.DocumentType = FieldLookupValueToLookupItem(item["DocumentType"]);
            file.MasterName = item["MasterName"] != null ? item["MasterName"].ToString() : "";
            file.FileName = item["FileLeafRef"].ToString();
            file.FilePath = item["FileRef"].ToString();

            return file;
        }

        private LookupItem FieldLookupValueToLookupItem(object field)
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


        /// <summary>
        /// Required CRM Identifier and CRM Name
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private bool ValidateUploadFields(SharePointDocument doc)
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

        private string DecodeAuthorizationString()
        {
            string userPass = string.Empty;
            if (Request.Headers.Authorization == null)
            {
                // throw HttpResponseHelper.GetUnauthorizedResponseException("Auth Header is null!");
                string emailAddress = ConfigurationManager.AppSettings["Email"].ToString();
                string password = ConfigurationManager.AppSettings["Password"].ToString();
                userPass = string.Format("{0}:{1}", emailAddress, password);
            }
            else
            {
                var authHeader = Request.Headers.Authorization;
                if (authHeader.Scheme.ToLower() != Constants.AUTH_HEADER.BASIC)
                {
                    throw HttpResponseHelper.GetUnauthorizedResponseException("Auth Header is not using BASIC scheme!");
                }
                var encodedUserPass = authHeader.Parameter;
                userPass = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUserPass));
            }
            return userPass;
        }

        private string SetEntityName(string entityName)
        {
            string rc = "";

            entityName = entityName.ToUpper();
            if (entityName.EndsWith("S"))
                rc = entityName + "ES";
            else
                rc = entityName + "S";

            return rc;
        }
    }
}
