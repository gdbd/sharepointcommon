using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class AddedReceiverAsync : ListEventReceiver<AddedItemAsync>
    {
        [Async(true)]
        public override void ItemAdded(AddedItemAsync addedItem)
        {
            AddedItemAsync.IsAddCalled = true;

            Assert.That(addedItem, Is.Not.Null);
            Assert.That(addedItem.Title, Is.EqualTo("test1"));
            Assert.That(addedItem.TheText, Is.EqualTo("test2"));

            AddedItemAsync.ManualResetEvent.Set();
        }
    }
}