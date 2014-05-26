using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class UpdatedReceiver : ListEventReceiver<UpdatedItem>
    {
        [Async(false)]
        public override void ItemUpdated(UpdatedItem addedItem)
        {
            UpdatedItem.IsUpdateCalled = true;

            try
            {
                Assert.That(addedItem, Is.Not.Null);
                Assert.That(addedItem.Title, Is.EqualTo("new title"));
                Assert.That(addedItem.TheText, Is.EqualTo("test2"));
            }
            catch (Exception e)
            {
                UpdatedItem.Exception = e;
            }
        }
    }
}