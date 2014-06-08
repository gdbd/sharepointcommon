using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Entity
{
    public enum TheChoice
    {
        Choice1,

        [Field("The Choice Number Two")]
        Choice2,

        Choice3,
    }
}