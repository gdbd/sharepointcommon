using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Events
{
    public class AddRemoveTestEventReceiver : ListEventReceiver<EventTestItem>
    {
        [Async(false)]
        protected override void ItemAdded(EventTestItem addedItem)
        {
            EventTestItem.IsAddCalled = true;
        }

        [Async(false)]
        protected override void ItemUpdated(EventTestItem updatedItem)
        {
            EventTestItem.IsUpdateCalled = true;
        }
    }
}