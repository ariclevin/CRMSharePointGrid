using Microsoft.SharePoint.Client;
using SPDocLibrary.Middleware.Models;
using SPDocLibrary.Middleware.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SPDocLibrary.Middleware.Controllers
{
    [EnableCors(origins: "*", headers: "Content-Type, Authorization", methods: "GET, POST, PUT, OPTIONS, DELETE")]
    public class LookupController : ApiController
    {
        // GET: api/Lookup/GetString/ListName
        // public IEnumerable<LookupItem> GetString([FromUri] string id) 
        [HttpGet]
        [Route("api/Lookup/GetAllListItems/{id}")]
        public HttpResponseMessage GetAllListItems([FromUri] string id)
        {
            try
            {
                string authorizationString = DecodeAuthorizationString();
                SPHelper.SetSharePointCredentials(authorizationString);
                ListItemCollection list = SPHelper.GetList(id);
                if (list != null)
                {
                    List<LookupItem> lookupList = new List<LookupItem>();
                    if (list.AreItemsAvailable)
                    {
                        foreach (ListItem item in list)
                        {
                            lookupList.Add(new LookupItem((int)item["ID"], item["Title"].ToString()));
                        }
                    }

                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(JsonConvert.SerializeObject(lookupList), Encoding.UTF8, "application/json");
                    return response;

                    // return lookupList;
                }
                else
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent(string.Format("List {0} was not found", id)),
                        ReasonPhrase = "List Not Found"
                    };
                    throw new HttpResponseException(resp);
                }

            }
            catch (System.Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(string.Format("Error Message: {0} was encountered", ex.Message)),
                    ReasonPhrase = "Error was Encountered"
                };
                throw new HttpResponseException(resp);
            }
        }

        #region OPTIONS method

        [HttpOptions]
        [Route("api/Lookup/GetString/{id}")]
        public HttpResponseMessage GetStringOptions([FromUri] string id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        #endregion

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
    }
}
