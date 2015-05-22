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
    public class DeletedDocReceiverAsync : ListEventReceiver<DeletedDocAsync>
    {
        [Async(true)]
        public override void ItemDeleted(int id)
        {
            try
            {
                DeletedDocAsync.DeletedId = id;
                DeletedDocAsync.IsDeleteCalled = true;
                DeletedDocAsync.ManualResetEvent.Set();
            }
            catch (Exception e)
            {
                DeletedDocAsync.Exception = e;
            }
        }
    }
}