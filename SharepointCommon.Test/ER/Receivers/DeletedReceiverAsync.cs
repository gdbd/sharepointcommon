using System;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class DeletedReceiverAsync : ListEventReceiver<DeletedItemAsync>
    {
        [Async(true)]
        public override void ItemDeleted(int id)
        {
            try
            {
                DeletedItemAsync.DeletedId = id;
                DeletedItemAsync.IsDeleteCalled = true;
                DeletedItemAsync.ManualResetEvent.Set();
            }
            catch (Exception e)
            {
                DeletedItemAsync.Exception = e;
            }
        }
    }
}