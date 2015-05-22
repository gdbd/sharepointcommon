using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class UpdatedDocReceiverAsync : ListEventReceiver<UpdatedDocAsync>
    {
        [Async(true)]
        public override void ItemUpdated(UpdatedDocAsync addedItem)
        {
            try
            {
                UpdatedDocAsync.IsUpdateCalled = true;
                UpdatedDocAsync.Received = addedItem;
            }
            catch (Exception e)
            {
                UpdatedDocAsync.Exception = e;
            }
        }
    }
}