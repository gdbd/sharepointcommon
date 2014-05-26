using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class AddingReceiverAsync : ListEventReceiver<AddingItemAsync>
    {
        [Async(true)]
        public override void ItemAdding(AddingItemAsync addedItem)
        {
        }
    }
}