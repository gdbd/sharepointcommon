﻿using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class AddRemoveTestEventReceiver : ListEventReceiver<AddRemoveTest>
    {
        [Async(false)]
        public override void ItemAdded(AddRemoveTest addedItem)
        {
            AddRemoveTest.IsAddCalled = true;

            Assert.That(addedItem, Is.Not.Null);
            Assert.That(addedItem.Title, Is.EqualTo("test1"));
            Assert.That(addedItem.TheText, Is.EqualTo("test2"));
        }

        [Async(false)]
        public override void ItemUpdated(AddRemoveTest updatedItem)
        {
            AddRemoveTest.IsUpdateCalled = true;
        }
    }
}