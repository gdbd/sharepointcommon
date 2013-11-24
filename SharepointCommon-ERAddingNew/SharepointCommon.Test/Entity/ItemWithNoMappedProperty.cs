using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Entity
{
    public class ItemWithNoMappedProperty : Item
    {
        [NotMapped]
        public string NotField { get; set; }

        [NotMapped]
        public string NotMapped { get; set; }
    }
}
