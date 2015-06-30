using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Entity
{
    public class NumberAsLookupTest : Item
    {
        [Field(LookupList = "ListForLookup")]
        public virtual Item CustomLookup { get; set; }
    }

    public class NumberAsLookupCreate : Item
    {
        public virtual double? CustomLookup { get; set; }
    }
}