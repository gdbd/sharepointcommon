// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    public abstract class ListEventReceiver<T> where T : Item, new()
    {
        protected virtual void ItemAdding(T addingItem) { }
        protected virtual void ItemAdded(T addedItem) { }
        protected virtual void ItemUpdating(T updatingItem, T changedItem) { }
        protected virtual void ItemUpdated(T updatedItem) { }
        protected virtual void ItemDeleting(T deletingItem) { }
        protected virtual void ItemDeleted(int deletedItemId) { }

        public bool Cancel { get; protected set; }
    }
}