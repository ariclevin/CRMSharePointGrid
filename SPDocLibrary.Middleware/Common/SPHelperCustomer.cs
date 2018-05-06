using SPDocLibrary.Middleware.Models;
using SPCAMLQueryBuilder;
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
        /// <summary>
        /// Retrieves All Customer Folders at the Root Level
        /// </summary>
        /// <returns></returns>
        public static ListItemCollection GetFolders()
        {
            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            if (spList != null && spList.ItemCount > 0)
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml =
                   @"<View>  
            <Query> 
               <Where><Eq><FieldRef Name='FSObjType' /><Value Type='Integer'>1</Value></Eq></Where> 
            </Query> 
             <ViewFields><FieldRef Name='FileLeafRef' /><FieldRef Name='ID' /><FieldRef Name='FileRef' /><FieldRef Name='FolderChildCount' /><FieldRef Name='_Level' /></ViewFields> 
      </View>";

                ListItemCollection listItems = spList.GetItems(camlQuery);
                ctx.Load(listItems);
                ctx.ExecuteQuery();
                return listItems;
            }
            else
                return null;
        }
        


        /// <summary>
        /// Retrieves List of Documents By the Master Id 
        /// </summary>
        /// <param name="masterId">Unique Identifer in String Format</param>
        /// <returns></returns>
        public static ListItemCollection GetDocumentsById(string masterId)
        {
            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            if (spList != null && spList.ItemCount > 0)
            {
                string whereClause =  String.Format(@"<Where>
                                        <Eq><FieldRef Name='MasterId' /><Value Type='Text'>{0}</Value></Eq>
                                        </Where> ", masterId);

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = String.Format(
                   @"<View Scope='RecursiveAll'>
            <Query> 
                {0}
            </Query> 
             <ViewFields><FieldRef Name='FileLeafRef' /><FieldRef Name='Title' /><FieldRef Name='MasterId' /><FieldRef Name='MasterNumber' /><FieldRef Name='MasterName' /><FieldRef Name='DocumentType' /><FieldRef Name='Created' /></ViewFields> 
      </View>", whereClause);

                ListItemCollection listItems = spList.GetItems(camlQuery);
                ctx.Load(listItems);
                ctx.ExecuteQuery();

                return listItems;
            }
            else
                return null;

        }

        /// <summary>
        /// Retrieves List of Documents By the Master Id 
        /// </summary>
        /// <param name="masterId">Unique Identifer in String Format</param>
        /// <returns></returns>
        public static ListItemCollection GetDocumentsByNumber(string masterNumber)
        {
            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            if (spList != null && spList.ItemCount > 0)
            {
                string whereClause = String.Format(@"<Where>
                                        <Eq><FieldRef Name='MasterNumber' /><Value Type='Text'>{0}</Value></Eq>
                                        </Where> ", masterNumber);

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = String.Format(
                   @"<View Scope='RecursiveAll'>
            <Query> 
                {0}
            </Query> 
             <ViewFields><FieldRef Name='FileLeafRef' /><FieldRef Name='Title' /><FieldRef Name='MasterId' /><FieldRef Name='MasterNumber' /><FieldRef Name='MasterName' /><FieldRef Name='DocumentType' /><FieldRef Name='Created' /></ViewFields> 
      </View>", whereClause);

                ListItemCollection listItems = spList.GetItems(camlQuery);
                ctx.Load(listItems);
                ctx.ExecuteQuery();

                // ctx.Dispose();

                return listItems;
            }
            else
                return null;

        }


        /// <summary>
        /// Retrieves List of Documents By Customer Id Type and Value
        /// </summary>
        /// <param name="customerIdType"></param>
        /// <param name="customerIdValue"></param>
        /// <returns></returns>
        public static ListItemCollection GetDocumentsByType(string masterId, string documentType)
        {
            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            int? documentTypeCode = GetListItemId("DocumentType", documentType);
            if (documentTypeCode.HasValue)
            {
                string whereClause = "";
                    whereClause = String.Format(@"<Where><And><And>
                                    <Eq><FieldRef Name='DocumentType' /><Value Type='Lookup'>{0}</Value></Eq>
                                    <Eq><FieldRef Name='MasterId' /><Value Type='Text'>{1}</Value></Eq></And>
                                    <Eq><FieldRef Name='FSObjType' /><Value Type='Integer'>0</Value></Eq></And>
                                </Where>  ", documentType, masterId);

                if (spList != null && spList.ItemCount > 0)
                {
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml =
                       String.Format(@"<View Scope='RecursiveAll'>  
                                <Query> 
                                    {0}
                                </Query> 
                         <ViewFields><FieldRef Name='FileLeafRef' /><FieldRef Name='Title' /><FieldRef Name='MasterId' /><FieldRef Name='MasterNumber' /><FieldRef Name='MasterName' /><FieldRef Name='DocumentType' /><FieldRef Name='Created' /></ViewFields> 
                          </View>", whereClause);

                    ListItemCollection listItems = spList.GetItems(camlQuery);
                    ctx.Load(listItems);
                    ctx.ExecuteQuery();

                    // ctx.Dispose();

                    return listItems;
                }
                else
                    return null;
            }
            else return null;
        }

        public static ListItemCollection GetDocuments(string fieldName, string fieldValue)
        {
            CAMLQueryFilter filter = new CAMLQueryFilter();
            if (!string.IsNullOrEmpty(fieldName))
            {
                SPCAMLQueryBuilder.FieldType fieldType = SharePointDocument.GetFieldTypeByFieldName(fieldName);
                    
                switch (fieldType)
                {
                    case SPCAMLQueryBuilder.FieldType.Lookup:
                        int intValue = int.MinValue;
                        bool isInt = int.TryParse(fieldValue, out intValue);
                        if (isInt)
                        {
                            filter = new CAMLQueryLookupFilter(fieldName, intValue, QueryType.Equal);
                        }
                        else
                        {
                            filter = new CAMLQueryLookupFilter(fieldName, fieldValue, QueryType.Equal);
                        }

                        break;
                    default:
                        filter = new CAMLQueryGenericFilter(fieldName, fieldType, fieldValue, QueryType.Equal);
                        break;
                }
            }

            CAMLQueryBuilder builder = new CAMLQueryBuilder(filter);


            builder.DocumentFilter(FSObjType.Document, true);

            builder.AddViewFields(SharePointDocument.GetAllFieldNames());

            builder.BuildQuery();
            builder.OrderBy("Created", false);
            builder.BuildViewFields();

            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = builder.ToString();

            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            if (spList != null && spList.ItemCount > 0)
            {
                ListItemCollection listItems = spList.GetItems(camlQuery);
                ctx.Load(listItems);
                ctx.ExecuteQuery();

                // ctx.Dispose();

                return listItems;
            }
            else
                return null;
        }

        public static int? GetListItemId(string listName, string listItemTitle)
        {
            int? rc = null;
            try
            {
                ClientContext ctx = ConnectToSharePoint();
                List spList = ctx.Web.Lists.GetByTitle(listName);
                ctx.Load(spList);
                ctx.ExecuteQuery();

                if (spList != null && spList.ItemCount > 0)
                {
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml =
                       String.Format(@"<View>  
            <Query> 
               <Where><Eq><FieldRef Name='Title' /><Value Type='Text'>{0}</Value></Eq></Where>
               <OrderBy><FieldRef Name='Title' /></OrderBy> 
            </Query> 
             <ViewFields><FieldRef Name='Title' /></ViewFields> 
      </View>", listItemTitle);

                    ListItemCollection listItems = spList.GetItems(camlQuery);
                    ctx.Load(listItems);
                    ctx.ExecuteQuery();

                    if (listItems.AreItemsAvailable)
                    {
                        if (listItems.Count > 0)
                        {
                            rc = Convert.ToInt32(listItems[0]["ID"]);
                        }
                    }

                }
                else
                    throw new Exception(String.Format("No items were found for list ", listName));

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return rc;
        }

        public static int? GetListItemId(string listName, string fieldName, string fieldValue)
        {
            int? rc = null;
            try
            {
                ClientContext ctx = ConnectToSharePoint();
                List spList = ctx.Web.Lists.GetByTitle(listName);
                ctx.Load(spList);
                ctx.ExecuteQuery();

                if (spList != null && spList.ItemCount > 0)
                {
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml =
                       String.Format(@"<View>  
            <Query> 
               <Where><Eq><FieldRef Name='{0}' /><Value Type='Text'>{1}</Value></Eq></Where>
               <OrderBy><FieldRef Name='Title' /></OrderBy> 
            </Query> 
             <ViewFields><FieldRef Name='Title' /></ViewFields> 
      </View>", fieldName, fieldValue);

                    ListItemCollection listItems = spList.GetItems(camlQuery);
                    ctx.Load(listItems);
                    ctx.ExecuteQuery();

                    if (listItems.AreItemsAvailable)
                    {
                        if (listItems.Count > 0)
                        {
                            rc = Convert.ToInt32(listItems[0]["ID"]);
                        }
                    }

                }
                else
                    throw new Exception(String.Format("No items were found for list ", listName));

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return rc;
        }


        public static void CreateRootFolder(string folderName)
        {
            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            ListItemCreationInformation folderCreateInfo = new ListItemCreationInformation
            {
                UnderlyingObjectType = FileSystemObjectType.Folder,
                LeafName = folderName
            };

            var folderItem = spList.AddItem(folderCreateInfo);
            folderItem.Update();
            ctx.ExecuteQuery();

        }

        public static ListItem CreateFolder(ListItem rootFolder, string rootFolderUrl, string childFolderName)
        {
            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            ListItemCreationInformation folderCreateInfo = new ListItemCreationInformation
            {
                UnderlyingObjectType = FileSystemObjectType.Folder,
                LeafName = childFolderName
            };

            Folder parentFolder = rootFolder.Folder;
            Folder folderItem = parentFolder.Folders.Add(String.Format(@"{0}/{1}", rootFolderUrl, childFolderName));
            folderItem.Update();
            ctx.ExecuteQuery();

            return folderItem.ListItemAllFields;
        }

        public static bool FolderExists(string folderName)
        {
            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            var folders = spList.GetItems(CamlQuery.CreateAllFoldersQuery());
            ctx.Load(spList.RootFolder);
            ctx.Load(folders);
            ctx.ExecuteQuery();
            var folderRelativeUrl = string.Format("/{0}/{1}", spList.RootFolder.ServerRelativeUrl, folderName);
            return Enumerable.Any(folders, folderItem => (string)folderItem["FileRef"] == folderRelativeUrl);
        }

        public static ListItem FindFolder(string folderName)
        {
            ListItem rc = null;
            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            if (spList != null && spList.ItemCount > 0)
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml =
                   String.Format(@"<View Scope='RecursiveAll'>  
            <Query> 
               <Where><And><Eq><FieldRef Name='FileLeafRef' /><Value Type='File'>{0}</Value></Eq><Eq><FieldRef Name='FSObjType' /><Value Type='Integer'>1</Value></Eq></And></Where> 
            </Query> 
             <ViewFields><FieldRef Name='FileLeafRef' /><FieldRef Name='ID' /><FieldRef Name='FileRef' /></ViewFields> 
      </View>", folderName);

                ListItemCollection listItems = spList.GetItems(camlQuery);
                ctx.Load(listItems);
                ctx.ExecuteQuery();

                if (listItems.AreItemsAvailable)
                {
                    if (listItems.Count > 0)
                        rc = listItems[0];
                }
            }

            return rc;
        }

        public static void RenameFolder(string sourceFolderName, string targetFolderName)
        {
            ClientContext ctx = ConnectToSharePoint();
            ListItem sourceFolder = FindFolder(sourceFolderName);

            sourceFolder["FileLeafRef"] = targetFolderName;
            sourceFolder.Update();
            ctx.ExecuteQuery();
        }

        public static ListItem FindFile(Guid uniqueId)
        {
            ListItem rc = null;
            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            if (spList != null && spList.ItemCount > 0)
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml =
                   String.Format(@"<View Scope='RecursiveAll'>  
            <Query> 
               <Where><And><Eq><FieldRef Name='UniqueId' /><Value Type='Guid'>{0}</Value></Eq>
            <Eq><FieldRef Name='FSObjType' /><Value Type='Integer'>0</Value></Eq></And></Where> 
            </Query> 
             <ViewFields><FieldRef Name='FileLeafRef' /><FieldRef Name='ID' /><FieldRef Name='FileRef' /><FieldRef Name='Activity_Status' /></ViewFields> 
      </View>", uniqueId.ToString());

                ListItemCollection listItems = spList.GetItems(camlQuery);
                ctx.Load(listItems);
                ctx.ExecuteQuery();

                if (listItems.AreItemsAvailable)
                {
                    if (listItems.Count > 0)
                        rc = listItems[0];
                }
            }

            return rc;

        }

        public static ListItem GetRootFolderForUpload(string folderName, bool isPlural)
        {
            ListItem rc = null;
            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            string rootFolderCriteria = "";
            if (isPlural)
                rootFolderCriteria = folderName.ToUpper();
            else
            {
                if (folderName.ToUpper().EndsWith("S"))
                    rootFolderCriteria = folderName.ToUpper() + "ES";
                else
                    rootFolderCriteria = folderName.ToUpper() + "S";
            }

            if (spList != null && spList.ItemCount > 0)
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml =
                   String.Format(@"<View>  
            <Query> 
               <Where><And><Contains><FieldRef Name='FileLeafRef' /><Value Type='File'>{0}</Value></Contains><Eq><FieldRef Name='FSObjType' /><Value Type='Integer'>1</Value></Eq></And></Where><OrderBy><FieldRef Name='Created' Ascending='FALSE' /><FieldRef Name='Created' Ascending='FALSE' /></OrderBy> 
            </Query> 
      </View>", rootFolderCriteria);

                ListItemCollection listItems = spList.GetItems(camlQuery);
                ctx.Load(listItems);
                ctx.ExecuteQuery();

                if (listItems.AreItemsAvailable)
                {
                    rc = listItems[0];
                }
            }
            return rc;
        }

        public static ListItem GetRootFolderForUpload(string folderType)
        {
            ListItem rc = null;
            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            string rootFolderCriteria = folderType;

            if (spList != null && spList.ItemCount > 0)
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml =
                   String.Format(@"<View>  
            <Query> 
               <Where><And><Contains><FieldRef Name='FileLeafRef' /><Value Type='File'>{0}</Value></Contains><Eq><FieldRef Name='FSObjType' /><Value Type='Integer'>1</Value></Eq></And></Where><OrderBy><FieldRef Name='Created' Ascending='FALSE' /><FieldRef Name='Created' Ascending='FALSE' /></OrderBy> 
            </Query> 
      </View>", rootFolderCriteria);

                ListItemCollection listItems = spList.GetItems(camlQuery);
                ctx.Load(listItems);
                ctx.ExecuteQuery();

                if (listItems.AreItemsAvailable)
                {
                    rc = listItems[0];
                }
            }
            return rc;
        }

        public static File UploadFile(string fileName, string folderName, byte[] fileBytes)
        {
            ClientContext ctx = ConnectToSharePoint();
            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            FileCreationInformation fileInfo = new FileCreationInformation()
            {
                ContentStream = new System.IO.MemoryStream(fileBytes),
                Url = fileName,
                Overwrite = true
            };

            ListItem liFolder = FindFolder(folderName);
            Folder folder = liFolder.Folder;

            File newFile = folder.Files.Add(fileInfo);
            ctx.Load(newFile);
            ctx.ExecuteQuery();

            return newFile;
        }

        public static byte[] DownloadFile(ListItem item)
        {
            ClientContext ctx = ConnectToSharePoint();
            List spList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            ClientResult<System.IO.Stream> result = item.File.OpenBinaryStream();
            ctx.ExecuteQuery();
            // result.Value;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                result.Value.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static void AddRequiredDocumentProperties(File newFile, string masterId, string masterNumber, string masterName, LookupItem documentType)
        {
            ClientContext ctx = ConnectToSharePoint();
            ListItem item = newFile.ListItemAllFields;

            ctx.Load(item);

            SetListItemText(item, "MasterId", masterId);
            SetListItemText(item, "MasterNumber", masterNumber);
            SetListItemText(item, "MasterName", masterName);
            SetListItemLookup(item, "DocumentType", documentType);

            item.Update();
            ctx.ExecuteQuery();
        }



        public static void AddDocumentProperties(ListItem item, SharePointDocument doc)
        {
            ClientContext ctx = ConnectToSharePoint();

            SetListItemText(item, "MasterId", doc.MasterId);
            SetListItemText(item, "MasterNumber", doc.MasterNumber);
            SetListItemText(item, "MasterName", doc.MasterName);
            SetListItemLookup(item, "DocumentType", doc.DocumentType);

            item.Update();
            ctx.ExecuteQuery();
        }



        private static void SetListItemText(ListItem item, string fieldName, string fieldValue)
        {
            if (!string.IsNullOrEmpty(fieldValue))
            {
                item[fieldName] = fieldValue;
            }
        }

        private static void SetListItemLookup(ListItem item, string fieldName, LookupItem lookup)
        {
            if (lookup != null)
            {
                if (!string.IsNullOrEmpty(lookup.Title))
                {
                    item[fieldName] = new FieldLookupValue() { LookupId = lookup.ItemId };
                }
            }
        }

        public static void DeleteFile(ListItem item)
        {
            ClientContext ctx = ConnectToSharePoint();
            item.DeleteObject();

            item.Update();
            ctx.ExecuteQuery();
        }

        private static List GetList(ClientContext ctx, string listName, int recurrence = 0)
        {
            List spList = ctx.Web.Lists.GetByTitle(listName);
            ctx.Load(spList);

            try
            {
                ctx.ExecuteQuery();

                if (spList != null && spList.ItemCount > 0)
                {
                    return spList;
                }
                else
                    return null;
            }
            catch (System.Exception ex)
            {
                if (recurrence == 0)
                    return GetList(ctx, listName, ++recurrence);
                else
                    return null;
            }
        }

        private static void CheckInDocument(ClientContext ctx, File newFile)
        {
            newFile.CheckIn("Document Checked After Upload", CheckinType.MajorCheckIn);
            ctx.ExecuteQuery();
        }

        private static string CheckFolderExists(ClientContext ctx, List spList, string folderName)
        {
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = "<View Scope='RecursiveAll'>"
                                                + "<Query>"
                                                         + "   <Where>"
 + "      <Eq><FieldRef Name='FSObjType' /><Value Type='Integer'>1</Value></Eq>"
                                                         + "   </Where>"
                                                + "</Query>"
                                + "</View>";
            ListItemCollection listItems = spList.GetItems(camlQuery);

            ctx.Load(listItems);
            ctx.ExecuteQuery();

            string folderPath = "";
            if (listItems.AreItemsAvailable)
            {
                folderPath = listItems[0]["FileRef"].ToString();
            }
            return folderPath;
        }



    }
}