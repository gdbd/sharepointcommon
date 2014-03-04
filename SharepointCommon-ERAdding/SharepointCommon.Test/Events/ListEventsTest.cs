using NUnit.Framework;

namespace SharepointCommon.Test.Events
{
    public class ListEventsTest
    {

        [Test]
        public void Add_Remove_Receiever_Test()
        {
            using (var ts = new TestListScope<EventTestItem>("Add_Remove_Receiever_Test"))
            {
                ts.List.AddEventReciver<AddRemoveTestEventReceiver>();
                var entity = new EventTestItem { Title = "test1", };
                ts.List.Add(entity);
                Assert.True(EventTestItem.IsAddCalled, "Not fired added receiver!");

                ts.List.RemoveEventReciver<AddRemoveTestEventReceiver>();
                entity.Title = "asd-asd";
                ts.List.Update(entity, true);
                Assert.False(EventTestItem.IsUpdateCalled, "Fired after receiver was removed!");
            }
        }
    }
}
