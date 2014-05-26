using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class AddingReceiver : ListEventReceiver<AddingItem>
    {
        [Async(false)]
        public override void ItemAdding(AddingItem addedItem)
        {
            AddingItem.IsAddCalled = true;

            Assert.That(addedItem, Is.Not.Null);
            Assert.That(addedItem.Title, Is.EqualTo("test1"));
            Assert.That(addedItem.TheText, Is.EqualTo("test2"));
        }
    }
}