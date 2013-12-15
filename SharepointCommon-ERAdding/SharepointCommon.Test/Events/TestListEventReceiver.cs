using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.Events
{
    public class TestListEventReceiver : ListEventReceiver<OneMoreField<string>>
    {
        public static bool IsCalled { get; set; }

        [Sequence(10000), Async]
        protected override void ItemAdded(OneMoreField<string> addedItem)
        {
            IsCalled = true;
        }

        protected override void ItemDeleted(int deletedItem)
        {
            IsCalled = true;
        }
    }
}