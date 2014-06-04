using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class UpdatedReceiverAsync : ListEventReceiver<UpdatedItemAsync>
    {
        [Async(true)]
        public override void ItemUpdated(UpdatedItemAsync addedItem)
        {
            try
            {
                UpdatedItemAsync.IsUpdateCalled = true;
                UpdatedItemAsync.Received = addedItem;
            }
            catch (Exception e)
            {
                UpdatedItemAsync.Exception = e;
            }
        }
    }
}