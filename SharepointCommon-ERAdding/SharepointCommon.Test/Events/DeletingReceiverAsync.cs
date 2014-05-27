using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class DeletingReceiverAsync : ListEventReceiver<DeletingItemAsync>
    {
        [Async(true)]
        public override void ItemDeleting(DeletingItemAsync addedItem)
        {
        }
    }
}