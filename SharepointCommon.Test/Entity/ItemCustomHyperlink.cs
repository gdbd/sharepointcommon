using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Entity
{
    public class ItemCustomHyperlink : Item
    {
        [Field(Required = true)]
        [CustomProperty("Type","URL")]
        public virtual string CustomField1 { get; set; }
    }
}