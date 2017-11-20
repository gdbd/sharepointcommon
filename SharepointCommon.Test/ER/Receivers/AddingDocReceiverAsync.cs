using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class AddingDocReceiverAsync : ListEventReceiver<AddingDocAsync>
    {
        [Async(true)]
        public override void ItemAdding(AddingDocAsync addedItem)
        {
        }
    }
}