// ReSharper disable once CheckNamespace

using SharepointCommon.Attributes;

namespace SharepointCommon
{
    /// <summary>
    /// Base class to inherit list event receivers.
    /// Need to override methods to hook events.
    /// Methods can be marked with attributes:
    /// <see cref="AsyncAttribute"/>,
    /// <see cref="SequenceAttribute"/>,
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ListEventReceiver<T> where T : Item, new()
    {
        public virtual void ItemAdding(T addingItem) { }
        public virtual void ItemAdded(T addedItem) { }
        public virtual void ItemUpdating(T originalItem, T changedItem) { }
        public virtual void ItemUpdated(T updatedItem) { }
        public virtual void ItemDeleting(T deletingItem) { }
        public virtual void ItemDeleted(int deletedItemId) { }
    }
}