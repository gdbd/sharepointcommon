using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class AddedDocReceiverAsync : ListEventReceiver<AddedDocAsync>
    {
        [Async(true)]
        public override void ItemAdded(AddedDocAsync addedItem)
        {
            try
            {
                AddedDocAsync.IsAddCalled = true;

                AddedDocAsync.Received = addedItem;

                AddedDocAsync.ManualResetEvent.Set();
            }
            catch (Exception e)
            {
                AddedDocAsync.Exception = e;
            }
        }
    }
}