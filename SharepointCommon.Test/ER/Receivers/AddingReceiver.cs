using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class AddingReceiver : ListEventReceiver<AddingItem>
    {
        [Async(false)]
        public override void ItemAdding(AddingItem addedItem)
        {
            try
            {
                AddingItem.IsAddCalled = true;

                AddingItem.Received = addedItem;
            }
            catch (Exception e)
            {
                AddingItem.Exception = e;
            }
        }
    }
}