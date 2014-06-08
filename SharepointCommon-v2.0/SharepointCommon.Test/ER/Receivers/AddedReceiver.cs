using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class AddedReceiver : ListEventReceiver<AddedItem>
    {
        [Async(false)]
        public override void ItemAdded(AddedItem addedItem)
        {
            try
            {
                AddedItem.IsAddCalled = true;

                AddedItem.Received = addedItem;
            }
            catch (Exception e)
            {
                AddedItem.Exception = e;
            }
        }
    }
}