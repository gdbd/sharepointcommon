using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class UpdatingReceiverAsync : ListEventReceiver<UpdatingItemAsync>
    {
        [Async(true)]
        public override void ItemUpdating(UpdatingItemAsync addedItem, UpdatingItemAsync second)
        {
            
        }
    }  
    
    public class UpdatingDocReceiverAsync : ListEventReceiver<UpdatingDocAsync>
    {
        [Async(true)]
        public override void ItemUpdating(UpdatingDocAsync addedItem, UpdatingDocAsync second)
        {
            
        }
    }
}