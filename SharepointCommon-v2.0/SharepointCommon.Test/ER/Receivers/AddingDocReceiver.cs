using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class AddingDocReceiver : ListEventReceiver<AddingDoc>
    {
        [Async(false)]
        public override void ItemAdding(AddingDoc addedItem)
        {
            try
            {
                AddingDoc.IsAddCalled = true;

                AddingDoc.Received = addedItem;
            }
            catch (Exception e)
            {
                AddingItem.Exception = e;
            }
        }
    }
}