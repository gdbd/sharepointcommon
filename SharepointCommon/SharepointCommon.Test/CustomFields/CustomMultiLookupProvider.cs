using Microsoft.SharePoint;
using NUnit.Framework;

namespace SharepointCommon.Test.CustomFields
{
    public class CustomMultiLookupProvider : CustomFieldProvider
    {
        public override string FieldTypeAsString
        {
            get { return "UniversalLookupFieldMulti"; }
        }

        public override SPListItem GetLookupItem(SPField field, object value)
        {
            Assert.NotNull(field);
            return null;
        }
    }
}