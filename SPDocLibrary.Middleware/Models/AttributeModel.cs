using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPDocLibrary.Middleware.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SharePointField : Attribute
    {
        private string _fieldName;
        SPCAMLQueryBuilder.FieldType _fieldType = SPCAMLQueryBuilder.FieldType.Unknown;

        public SharePointField(string fieldName, SPCAMLQueryBuilder.FieldType fieldType)
        {
            _fieldName = fieldName;
            _fieldType = fieldType;
        }

        public virtual string FieldName
        {
            get { return _fieldName; }
        }

        public virtual SPCAMLQueryBuilder.FieldType FieldType
        {
            get { return _fieldType;  }
        }
    }
}