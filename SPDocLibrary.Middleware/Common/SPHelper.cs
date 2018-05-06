using SPDocLibrary.Middleware.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.SharePoint.Client;

namespace SPDocLibrary.Middleware.Common
{
    public partial class SPHelper
    {

        public static string SITE_URL { get; set; }
        public static string WEB_FULL_URL { get; set; }
        public static ClientContext Context { get; set; }
        public static AuthenticationContext AuthContext { get; set; }

        public static string AccessToken { get; set; }

        public static string ServiceUserName { get; set; }

        public static string ServicePassword { get; set; }

        private static void ConnectToSharePoint(string url, string fullPath)
        {
            using (ClientContext ctx = new ClientContext(url))
            {
                SecureString securePassword = new SecureString();
                string emailAddress = ConfigurationManager.AppSettings["Email"].ToString();
                string password = CryptoHelper.Decrypt("SharePnt", ConfigurationManager.AppSettings["Password"].ToString());

                foreach (char c in password.ToCharArray()) securePassword.AppendChar(c);
                ctx.Credentials = new SharePointOnlineCredentials(emailAddress, securePassword);

                Web web = ctx.Web;
                ctx.Load(web);

                List docs = web.Lists.GetByTitle("Documents");
                ctx.Load(docs);

                Folder folder = docs.RootFolder;
                ctx.Load(folder);

                FileCollection files = folder.Files;
                ctx.Load(files);
                ctx.ExecuteQuery();
            }
        }

        #region SharePoint Connection Methods
        public static void SetSharePointCredentials(string authorization)
        {
            string[] authCredentials = authorization.Split(':');
            ServiceUserName = authCredentials[0].ToString();
            ServicePassword = authCredentials[1].ToString();
        }

        private static ClientContext ConnectToSharePoint()
        {
            if ((Context != null) && (!string.IsNullOrEmpty(Context.Url)))
                return Context;
            else
            {
                return ConnectToSharePointLive();
            }

        }

        private static ClientContext ConnectToSharePointLive()
        {
            SITE_URL = ConfigurationManager.AppSettings["SITE_URL"].ToString();  
            WEB_FULL_URL = ConfigurationManager.AppSettings["WEB_FULL_URL"].ToString(); 
            Context = new ClientContext(WEB_FULL_URL);

            SecureString securePassword = new SecureString();
            if (!string.IsNullOrEmpty(ServiceUserName))
            {

                foreach (char c in ServicePassword.ToCharArray()) securePassword.AppendChar(c);
                Context.Credentials = new SharePointOnlineCredentials(ServiceUserName, securePassword);
            }
            else
            {
                string emailAddress = ConfigurationManager.AppSettings["Email"].ToString();
                string password = ConfigurationManager.AppSettings["Password"].ToString();
                // string password = CryptoHelper.Decrypt("SharePnt", ConfigurationManager.AppSettings["Password"].ToString());

                foreach (char c in password.ToCharArray()) securePassword.AppendChar(c);
                Context.Credentials = new SharePointOnlineCredentials(emailAddress, securePassword);
            }

            return Context;
        }

        private static void Disconnect()
        {
            Context.Dispose();
            Context = null;
        }

        #endregion

        public static ListItemCollection GetList(string listName)
        {
            try
            {
                ClientContext ctx = ConnectToSharePoint();
                List spList = GetList(ctx, listName);

                if (spList != null)
                {
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml =
                       @"<View>  
            <Query> 
               <OrderBy><FieldRef Name='Title' /></OrderBy> 
            </Query> 
             <ViewFields><FieldRef Name='Title' /></ViewFields> 
      </View>";

                    ListItemCollection listItems = spList.GetItems(camlQuery);
                    ctx.Load(listItems);
                    ctx.ExecuteQuery();
                    return listItems;
                }
                else
                    throw new Exception(String.Format("No items were found for list ", listName));

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static int GetChildFoldersCount(string path)
        {
            ClientContext ctx = ConnectToSharePoint();

            string fullPath = SITE_URL + path;
            Folder fldr = ctx.Web.GetFolderByServerRelativeUrl(path);
            FolderCollection childFolders = fldr.Folders;
            ctx.Load(fldr);
            ctx.Load(childFolders);
            ctx.ExecuteQuery();

            return childFolders.Count;
        }


        public static int GetChildDocumentCount(string path)
        {
            ClientContext ctx = ConnectToSharePoint();

            string fullPath = SITE_URL + path;
            Folder fldr = ctx.Web.GetFolderByServerRelativeUrl(path);
            FileCollection childDocuments = fldr.Files; // .Count<Folder>();
            ctx.Load(fldr);
            ctx.Load(childDocuments);
            ctx.ExecuteQuery();

            return childDocuments.Count;
        }
    }
}