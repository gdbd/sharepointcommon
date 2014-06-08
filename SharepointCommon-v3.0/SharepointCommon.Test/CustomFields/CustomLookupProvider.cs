using Microsoft.SharePoint;
using NUnit.Framework;

namespace SharepointCommon.Test.CustomFields
{
    public class CustomLookupProvider : CustomFieldProvider
    {
        public override string FieldTypeAsString
        {
            get { return "UniversalLookupField"; }
        }

        public override SPListItem GetLookupItem(SPField field, object value)
        {
            Assert.NotNull(field);
            return null;
        }
    }
}