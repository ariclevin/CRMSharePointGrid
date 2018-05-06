using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace SPDocLibrary.Middleware.Models
{
    public class SharePointDocument
    {
        [SharePointField("UniqueId", SPCAMLQueryBuilder.FieldType.Text)]
        public Guid FileId { get; set; }

        [SharePointField("FileLeafRef", SPCAMLQueryBuilder.FieldType.Text)]
        public string FileName { get; set; }

        [SharePointField("FileRef", SPCAMLQueryBuilder.FieldType.Text)]
        public string FilePath { get; set; }

        [SharePointField("MasterId", SPCAMLQueryBuilder.FieldType.Text)]
        public string MasterId { get; set; }

        [SharePointField("MasterNumber", SPCAMLQueryBuilder.FieldType.Text)]
        public string MasterNumber { get; set; }

        [SharePointField("MasterName", SPCAMLQueryBuilder.FieldType.Text)]
        public string MasterName { get; set; }

        [SharePointField("DocumentType", SPCAMLQueryBuilder.FieldType.Lookup)]
        public LookupItem DocumentType { get; set; } // Agreement, Invoice, License, Certificate

        public static string GetAttributeName(Type t)
        {
            string rc = "";

            // Get instance of the attribute.
            SharePointField MyAttribute =
                (SharePointField)Attribute.GetCustomAttribute(t, typeof(SharePointField));

            if (MyAttribute == null)
            {
                rc = "";
            }
            else
            {
                rc = MyAttribute.FieldName;
            }

            return rc;
        }

        public static SPCAMLQueryBuilder.FieldType GetAttributeType(Type t)
        {
            SPCAMLQueryBuilder.FieldType rc = SPCAMLQueryBuilder.FieldType.Unknown;

            // Get instance of the attribute.
            SharePointField MyAttribute =
                (SharePointField)Attribute.GetCustomAttribute(t, typeof(SharePointField));

            if (MyAttribute != null)
            {
                rc = MyAttribute.FieldType;
            }

            return rc;
        }

        public static SPCAMLQueryBuilder.FieldType GetFieldTypeByFieldName(string fieldName)
        {
            PropertyInfo[] props = typeof(SharePointDocument).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    SharePointField field = attr as SharePointField;
                    if (field != null)
                    {
                        string propFieldName = field.FieldName;
                        if (propFieldName == fieldName)
                        {
                            SPCAMLQueryBuilder.FieldType fieldType = field.FieldType;
                            return fieldType;
                        }
                    }
                }
            }

            return SPCAMLQueryBuilder.FieldType.Unknown;
        }

        public static List<string> GetAllFieldNames()
        {
            List<string> fieldNames = new List<string>();

            PropertyInfo[] props = typeof(SharePointDocument).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    SharePointField field = attr as SharePointField;
                    if (field != null)
                    {
                        string propFieldName = field.FieldName;
                        fieldNames.Add(propFieldName);
                    }
                }
            }

            return fieldNames;
        }

        public static string GetPropertyName(string propertyName)
        {
            string rc = "";

            PropertyInfo prop = typeof(SharePointDocument).GetProperty(propertyName);
            object[] attrs = prop.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                SharePointField field = attr as SharePointField;
                if (field != null)
                {
                    rc = field.FieldName;
                    break;
                }
            }
            return rc;
        }
    }


}