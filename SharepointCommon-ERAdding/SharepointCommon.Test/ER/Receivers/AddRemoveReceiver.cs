using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class AddRemoveReceiver : ListEventReceiver<AddRemoveTest>
    {
        [Async(false)]
        public override void ItemAdded(AddRemoveTest addedItem)
        {
            try
            {
                AddRemoveTest.IsAddCalled = true;

                Assert.That(addedItem, Is.Not.Null);
                Assert.That(addedItem.Title, Is.EqualTo("test1"));
                Assert.That(addedItem.TheText, Is.EqualTo("test2"));
            }
            catch (Exception e)
            {
                AddRemoveTest.Exception = e;
            }
        }

        [Async(false)]
        public override void ItemUpdated(AddRemoveTest updatedItem)
        {
            AddRemoveTest.IsUpdateCalled = true;
        }
    }
}