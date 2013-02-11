using System;
using NUnit.Framework;
using SharepointCommon.Attributes;

namespace SharepointCommon.Test
{
    public class EventReceiversTest
    {
        private readonly string _webUrl = string.Format("http://{0}/", Environment.MachineName);

        [Test]
        public void Add_EventReceiver_Works_Test()
        {
            using (var wf = WebFactory.Open(_webUrl))
            {
                IQueryList<Item> list = null;
                try
                {
                    list = wf.Create<Item>("Add_EventReceiver_Works_Test");

                    list.Events.Add<TestEventReceiver>(er => er.ItemAdded, er => er.ItemDeleted);
                    list.Add(new Item { Title = "Add_EventReceiver_Works_Test" });
                    Assert.That(TestEventReceiver.IsAddedCalled);

                    list.Events.Remove<TestEventReceiver>(er => er.ItemAdded, er => er.ItemDeleted);
                    TestEventReceiver.IsAddedCalled = false;
                    list.Add(new Item { Title = "Add_EventReceiver_Works_Test2" });
                    Assert.That(!TestEventReceiver.IsAddedCalled);
                }
                finally
                {
                    if (list != null) list.DeleteList(false);
                }
            }
        }
    }

    public class TestEventReceiver : Events.ListEventHandler
    {
        public static bool IsAddedCalled { get; set; }

        [Sequence(10000)]
        public override void ItemAdded(Item addedItem)
        {
            IsAddedCalled = true;
        }

        public override void ItemDeleted(Item deletedItem)
        {
        }
    }
}
