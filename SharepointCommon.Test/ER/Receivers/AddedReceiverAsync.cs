using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class AddedReceiverAsync : ListEventReceiver<AddedItemAsync>
    {
        [Async(true)]
        public override void ItemAdded(AddedItemAsync addedItem)
        {
            try
            {
                AddedItemAsync.IsAddCalled = true;

                AddedItemAsync.Received = addedItem;

                AddedItemAsync.ManualResetEvent.Set();
            }
            catch (Exception e)
            {
                AddedItemAsync.Exception = e;
            }
        }
    }
}