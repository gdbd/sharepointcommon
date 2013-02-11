namespace SharepointCommon.Events
{
    public abstract class ListEventHandler
    {
        public virtual void ItemAdded(Item addedItem) { }
        public virtual void ItemDeleted(Item deletedItem) { }
    }
}