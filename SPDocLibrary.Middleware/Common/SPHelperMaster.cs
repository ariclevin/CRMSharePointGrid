using SPDocLibrary.Middleware.Models;
using CAML = Pepsi.SP.CAML.QueryBuilder;
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
        public static void AddRequiredMasterDocumentProperties(File newFile, MasterDocument doc)
        {
            ClientContext ctx = ConnectToSharePoint();
            ListItem item = newFile.ListItemAllFields;

            ctx.Load(item);

            var props = typeof(SharePointField).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(SharePointField)));

            SetListItemLookup(item, MasterDocument.GetAttributeName(doc.BusinessSector.GetType()), doc.BusinessSector);
            SetListItemLookup(item, MasterDocument.GetAttributeName(doc.BusinessDivision.GetType()), doc.BusinessDivision);
            SetListItemLookup(item, MasterDocument.GetAttributeName(doc.BusinessDivisionCategory.GetType()), doc.BusinessDivisionCategory);

            item.Update();
            ctx.ExecuteQuery();
        }

        public static void AddMasterDocumentProperties(ListItem item, MasterDocument doc)
        {
            ClientContext ctx = ConnectToSharePoint();

            SetListItemText(item, "MSA_ID", doc.MasterDocumentId);
            SetListItemLookup(item, "CUSTOMER_ID_TYPE", doc.MasterDocumentType);
            SetListItemText(item, "CUSTOMER_ID_VAL", doc.MasterDocumentName);
            SetListItemLookup(item, "DOCUMENT_CATEGORY", doc.BusinessSector);
            SetListItemLookup(item, "DOCUMENT_SUBCATEGORY", doc.BusinessDivision);
            SetListItemLookup(item, "BUSINESS_DIVISION_CATEGORY", doc.BusinessDivisionCategory);
            SetListItemText(item, "CREATEDBY_ID", doc.CreatedBy.Key);
            SetListItemText(item, "CREATEDBY_NAME", doc.CreatedBy.Value);
            item["Activity_Status"] = true;

            item.Update();
            ctx.ExecuteQuery();
        }

        public static ListItemCollection GetMasterDocuments(List<KeyValuePair<string, string>> list)
        {
            // When using For Each Need to create initial builder with value
            CAML.QueryBuilder.CAMLQueryGenericFilter filterAll = new CAML.QueryBuilder.CAMLQueryGenericFilter("ACTIVITY_STATUS", CAML.QueryBuilder.FieldType.Boolean, "1", CAML.QueryBuilder.QueryType.Equal);
            CAML.QueryBuilder.CAMLQueryBuilder builder = new CAML.QueryBuilder.CAMLQueryBuilder(filterAll);

            foreach (KeyValuePair<string, string> kvp in list)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    CAML.QueryBuilder.FieldType fieldType = MasterDocument.GetFieldTypeByFieldName(kvp.Key);
                    CAML.QueryBuilder.CAMLQueryFilter filter;
                    switch (fieldType)
                    {
                        case CAML.QueryBuilder.FieldType.Lookup:
                            int intValue = int.MinValue;
                            bool isInt = int.TryParse(kvp.Value, out intValue);
                            if (isInt)
                            {
                                filter = new CAML.QueryBuilder.CAMLQueryLookupFilter(kvp.Key, intValue, CAML.QueryBuilder.QueryType.Equal);
                                builder.ANDFilter(filter);
                            }
                            else
                            {
                                filter = new CAML.QueryBuilder.CAMLQueryLookupFilter(kvp.Key, kvp.Value, CAML.QueryBuilder.QueryType.Equal);
                                builder.ANDFilter(filter);
                            }

                            break;
                        default:
                            filter = new CAML.QueryBuilder.CAMLQueryGenericFilter(kvp.Key, fieldType, kvp.Value, CAML.QueryBuilder.QueryType.Equal);
                            builder.ANDFilter(filter);
                            break;
                    }
                }
            } // foreach

            builder.DocumentFilter(CAML.QueryBuilder.FSObjType.Document, true);

            builder.AddViewFields(MasterDocument.GetAllFieldNames());

            builder.BuildQuery();
            builder.OrderBy("Created", false);
            builder.BuildViewFields();

            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = builder.ToString();

            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("MasterDocuments");
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

        public static string GetBusinessDivisionCategoryFolder(int id)
        {
            string rc = "";
            ClientContext ctx = ConnectToSharePoint();

            List spList = ctx.Web.Lists.GetByTitle("MasterDocuments");
            ctx.Load(spList);
            ctx.ExecuteQuery();

            if (spList != null && spList.ItemCount > 0)
            {
                Microsoft.SharePoint.Client.CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml =
                   String.Format(@"<View>  
            <Query> 
               <Where><Eq><FieldRef Name='ID' /><Value Type='Counter'>{0}</Value></Eq></Where> 
            </Query> 
             <ViewFields><FieldRef Name='Title' /><FieldRef Name='FolderRef' /></ViewFields> 
      </View>", id.ToString());

                ListItemCollection listItems = spList.GetItems(camlQuery);
                ctx.Load(listItems);
                ctx.ExecuteQuery();

                if (listItems.AreItemsAvailable)
                {
                    if (listItems.Count > 0)
                    {
                        rc = listItems[0]["FolderRef"].ToString();
                    }
                }

            }
            else
                throw new Exception("No items were found for this BUSINESS DIVISION CATEGORY ");

            return rc;
        }

        public static ListItem GetMasterFolderForUpload(string relativeUrl)
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
               <Where><And><And><BeginsWith><FieldRef Name='FileLeafRef' /><Value Type='File'>DOCS</Value></BeginsWith><BeginsWith><FieldRef Name='FileRef' /><Value Type='Lookup'>{0}</Value></BeginsWith></And><Eq><FieldRef Name='FSObjType' /><Value Type='Integer'>1</Value></Eq></And></Where><OrderBy><FieldRef Name='Created' Ascending='FALSE' /><FieldRef Name='Created' Ascending='FALSE' /></OrderBy> 
            </Query> 
             <ViewFields><FieldRef Name='FileLeafRef' /></ViewFields> 
      </View>", relativeUrl);

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
    }
}