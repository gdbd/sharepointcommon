using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Entity
{
    public class ChoiceAsStringTest : Item
    {
        public virtual string CustomChoice { get; set; }
    }

    public class ChoiceAsStringCreate : Item
    {
        public virtual TheChoice? CustomChoice { get; set; }
    }
}