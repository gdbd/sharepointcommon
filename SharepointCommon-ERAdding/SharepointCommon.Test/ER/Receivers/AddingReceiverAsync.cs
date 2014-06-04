using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class AddingReceiverAsync : ListEventReceiver<AddingItemAsync>
    {
        [Async(true)]
        public override void ItemAdding(AddingItemAsync addedItem)
        {
        }
    }
}