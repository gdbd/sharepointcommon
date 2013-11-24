using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Entity
{

    public class LookupWithShowField : Item
    {
        [Field(LookupList = "ListForLookup")]
        public virtual Item CustomLookup { get; set; }

        [Field(LookupList = "ListForLookup", LookupField = "ID")]
        public virtual Item CustomLookupWithShowField { get; set; }
    }
}