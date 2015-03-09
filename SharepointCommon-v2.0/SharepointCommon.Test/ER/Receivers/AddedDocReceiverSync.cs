using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class AddedDocReceiverSync : ListEventReceiver<AddedDocSync>
    {
        public override void ItemAdded(AddedDocSync addedItem)
        {
            try
            {
                AddedDocSync.IsAddCalled = true;

                AddedDocSync.Received = addedItem;

                AddedDocSync.ManualResetEvent.Set();
            }
            catch (Exception e)
            {
                AddedDocSync.Exception = e;
            }
        }
    }
}