using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class UpdatingReceiver : ListEventReceiver<UpdatingItem>
    {
        [Async(false)]
        public override void ItemUpdating(UpdatingItem addedItem, UpdatingItem second)
        {
            UpdatingItem.IsUpdateCalled = true;

            try
            {
                Assert.That(addedItem, Is.Not.Null);
                Assert.That(second, Is.Not.Null);
                Assert.That(addedItem.Title, Is.EqualTo("test1"));
                Assert.That(second.Title, Is.EqualTo("new title"));
                Assert.That(addedItem.TheText, Is.EqualTo("test2"));
            }
            catch (Exception e)
            {
                UpdatingItem.Exception = e;
            }
        }
    }
}