using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Entity
{
    public class ItemWithNoMappedProperty : Item
    {
        [NotField]
        public string NotField { get; set; }

        [NotMapped]
        public string NotMapped { get; set; }
    }
}
