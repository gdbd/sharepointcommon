using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class DeletedReceiver : ListEventReceiver<DeletedItem>
    {
        [Async(false)]
        public override void ItemDeleted(int id)
        {
            DeletedItem.DeletedId = id;
            DeletedItem.IsDeleteCalled = true;
        }
    }
}