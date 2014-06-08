// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    public abstract class ListEventReceiver<T> where T : Item, new()
    {
        public virtual void ItemAdding(T addingItem) { }
        public virtual void ItemAdded(T addedItem) { }
        public virtual void ItemUpdating(T originalItem, T changedItem) { }
        public virtual void ItemUpdated(T updatedItem) { }
        public virtual void ItemDeleting(T deletingItem) { }
        public virtual void ItemDeleted(int deletedItemId) { }

      //  public bool Cancel { get; protected set; }
    }
}