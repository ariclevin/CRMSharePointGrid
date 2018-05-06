using SPDocLibrary.Middleware.Common;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace SPDocLibrary.Middleware
{
    public partial class Auth : System.Web.UI.Page
    {
        public static AuthenticationContext AuthContext { get; set; }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Params.GetValues("code") != null)
            {
                AuthenticationResult ar = GetAccessToken(Request.Params.GetValues("code")[0], Constants.ClientId, Constants.ClientSecret, Constants.ReturnUrl.ToString());
                //AuthorizationResultSession arSession = new AuthorizationResultSession();
                //arSession.AccessToken = ar.AccessToken;
                //arSession.RefreshToken = ar.RefreshToken;
                //arSession.UserInfo = ar.UserInfo.GivenName;
                //arSession.ExpiresOn = ar.ExpiresOn;
                //Session["AuthenticationResult"] = arSession;

                Response.Redirect(String.Format("{0}#{1}", ConfigurationManager.AppSettings["CRMWebResourceUrl"].ToString(), ar.AccessToken));
            }
            else
                GetAuthorizationCode(Constants.ClientId, Constants.ReturnUrl.ToString());
        }

        private string GetToken(string authority, string siteUrl)
        {
            AuthenticationResult ar = AcquireTokenAsync(authority, GetSharePointHost(siteUrl));
            Response.Write(ar.AccessToken);
            return ar.AccessToken;
        }

        private void GetAuthorizationCode(string clientId, string redirectUri)
        {
            var @params = new NameValueCollection
            {
                {"response_type", "code"},
                {"client_id", clientId},
                {"resource", "https://pepsico.sharepoint.com"},
                { "redirect_uri", redirectUri}
            };

            //Create sign-in query string
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString.Add(@params);

            Response.Redirect(String.Format("https://login.windows.net/common/oauth2/authorize?{0}", queryString));
        }

        public AuthenticationResult GetAccessToken(string authorizationCode, string clientID, string clientSecret, string redirectUri)
        {
            //Redirect uri must match the redirect_uri used when requesting Authorization code.
            //Note: If you use a redirect back to Default, as in this sample, you need to add a forward slash
            //such as http://localhost:13526/

            // Get auth token from auth code       
            TokenCache TC = new TokenCache();

            //Values are hard-coded for sample purposes
            string authority = "https://login.windows.net/common/oauth2/authorize";
            AuthenticationContext AC = new AuthenticationContext(authority, TC);
            ClientCredential cc = new ClientCredential(clientID, clientSecret);

            //Set token from authentication result
            return AC.AcquireTokenByAuthorizationCode(authorizationCode, new Uri(redirectUri), cc);
        }

        private AuthenticationResult AcquireTokenAsync(string authContextUrl, string resourceId)
        {
            AuthenticationResult ar = null;

            try
            {
                //create a new authentication context for our app
                AuthContext = new AuthenticationContext(authContextUrl);

                //look to see if we have an authentication context in cache already
                //we would have gotten this when we authenticated previously
                if (AuthContext.TokenCache.ReadItems().Count() > 0)
                {

                    //re-bind AuthenticationContext to the authority source of the cached token.
                    //this is needed for the cache to work when asking for a token from that authority.
                    string cachedAuthority =
                        AuthContext.TokenCache.ReadItems().First().Authority;

                    AuthContext = new AuthenticationContext(cachedAuthority);
                }

                //try to get the AccessToken silently using the resourceId that was passed in
                //and the client ID of the application.
                // ar = (await AuthContext.AcquireTokenSilent(resourceId, Constants.ClientId));
            }
            catch (Exception)
            {
                //not in cache; we'll get it with the full oauth flow
            }

            if (ar == null)
            {
                try
                {
                    ar = AuthContext.AcquireToken(resourceId, Constants.ClientId, Constants.ReturnUrl, PromptBehavior.Always);

                    //DiscoveryClient dc = new DiscoveryClient(() => ar.AccessToken);
                    //CapabilityDiscoveryResult cdr = await dc.DiscoverCapabilityAsync("MyFiles");
                }
                catch (Exception acquireEx)
                {
                    //utter failure here, we need let the user know we just can't do it
                    // MessageBox.Show("Error trying to acquire authentication result: " + acquireEx.Message);
                    throw new Exception("Error trying to acquire authentication result: " + acquireEx.Message);
                }
            }

            return ar;
        }

        private static string GetSharePointHost(string url)
        {
            Uri theHost = new Uri(url);
            return theHost.Scheme + "://" + theHost.Host + "/";
        }
    }

    public class AuthorizationResultSession
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public string UserInfo { get; set; }

        public DateTimeOffset ExpiresOn { get; set; }
    }
}