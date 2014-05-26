using System;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.Events
{
    public class TestListEventReceiver : ListEventReceiver<OneMoreField<string>>
    {
        public static bool IsCalled { get; set; }

        public override void ItemAdding(OneMoreField<string> addingItem)
        {
            addingItem.Author = new User();
            addingItem.ConcreteParentList = null;
            addingItem.Created = DateTime.Now.AddDays(4);
            addingItem.Editor = new User("Asd");
            addingItem.Guid = new Guid();
            addingItem.Modified=new DateTime();
            addingItem.ListItem = null;
            addingItem.ParentList = null;
            addingItem.Version = new Version();
            addingItem.AdditionalField = "TestItem_Adding";
        }

        [Sequence(10000), Async(false)]
        public override void ItemAdded(OneMoreField<string> addedItem)
        {
            IsCalled = true;
            addedItem.Title = "TestItem_Added";
            addedItem.ParentList.Update(addedItem, false, i => i.Title);
        }

        [Async(false)]
        public override void ItemUpdated(OneMoreField<string> updatedItem)
        {
            base.ItemUpdated(updatedItem);
            updatedItem.Title = "ItemUpdated";
            updatedItem.ParentList.Update(updatedItem, false, u => u.Title);
        }

        public override void ItemUpdating(OneMoreField<string> updatingItem, OneMoreField<string> changedItem)
        {
            base.ItemUpdating(updatingItem, changedItem);
            var s = updatingItem.Title;
            updatingItem.AdditionalField = "ItemUpdating";
        }

        public override void ItemDeleting(OneMoreField<string> deletingItem)
        {
            base.ItemDeleting(deletingItem);
        }
        [Async(false)]
        public override void ItemDeleted(int deletedItem)
        {
            base.ItemDeleted(deletedItem);
        }
    }
}