using System;
using Microsoft.SharePoint;

namespace SharepointCommon.Common
{
    internal class Mockable
    {
        internal static Func<SPFieldCollection, string, string> AddFieldAsXml = (collection, s) => collection.AddFieldAsXml(s);
        internal static Func<SPFieldCollection, string, SPField> GetFieldByInternalName = (collection, s) => collection.GetFieldByInternalName(s);
        internal static Action<SPField, Field> FieldMapper_SetFieldProperties = (field, field1) => FieldMapper.SetFieldProperties(field, field1);
        internal static Action<SPListItem, string, object> SetListItemValue = (item, s, arg3) => item[s] = arg3; 
    }
}
