using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class DeletingReceiverAsync : ListEventReceiver<DeletingItemAsync>
    {
        [Async(true)]
        public override void ItemDeleting(DeletingItemAsync addedItem)
        {
        }
    }
}