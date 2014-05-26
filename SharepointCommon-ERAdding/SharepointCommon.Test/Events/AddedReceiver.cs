using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class AddedReceiver : ListEventReceiver<AddedItem>
    {
        [Async(false)]
        public override void ItemAdded(AddedItem addedItem)
        {
            AddedItem.IsAddCalled = true;

            Assert.That(addedItem, Is.Not.Null);
            Assert.That(addedItem.Title, Is.EqualTo("test1"));
            Assert.That(addedItem.TheText, Is.EqualTo("test2"));
        }
    }
}