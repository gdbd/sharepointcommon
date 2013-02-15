using SharepointCommon.Attributes;
using SharepointCommon.Events;

namespace SharepointCommon.Test.Events
{
    public class ListEventReceiver : ListEventHandler
    {
        public static bool IsCalled { get; set; }

        [Sequence(10000), Async]
        public override void ItemAdded(Item addedItem)
        {
            IsCalled = true;
        }

        public override void ItemDeleted(Item deletedItem)
        {
            IsCalled = true;
        }
    }
}