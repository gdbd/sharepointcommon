using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class UpdatedReceiverAsync : ListEventReceiver<UpdatedItemAsync>
    {
        [Async(true)]
        public override void ItemUpdated(UpdatedItemAsync addedItem)
        {
            UpdatedItemAsync.IsUpdateCalled = true;

            try
            {
                Assert.That(addedItem, Is.Not.Null);
                Assert.That(addedItem.Title, Is.EqualTo("new title"));
                Assert.That(addedItem.TheText, Is.EqualTo("test2"));
            }
            catch (Exception e)
            {
                UpdatedItemAsync.Exception = e;
            }
        }
    }
}