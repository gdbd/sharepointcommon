namespace SharepointCommon.Test.CustomFields
{
    public class CustomLookupProviderBad : CustomFieldProvider
    {
        public override string FieldTypeAsString
        {
            get { return "UniversalLookupField"; }
        }
    }
}