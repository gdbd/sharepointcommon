using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class DeletedReceiverAsync : ListEventReceiver<DeletedItemAsync>
    {
        [Async(true)]
        public override void ItemDeleted(int id)
        {
            DeletedItemAsync.DeletedId = id;
            DeletedItemAsync.IsDeleteCalled = true;
            DeletedItemAsync.ManualResetEvent.Set();
        }
    }
}