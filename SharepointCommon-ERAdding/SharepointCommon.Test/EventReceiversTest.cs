using System;
using NUnit.Framework;

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
                    list.Events.Add += TestEventReceiver.Added;

                    list.Add(new Item { Title = "Add_EventReceiver_Works_Test" });

                    Assert.That(TestEventReceiver.IsAddedCalled);
                }
                finally
                {
                    if (list != null) list.DeleteList(false);
                }
            }
        }
    }

    public class TestEventReceiver
    {
        public static bool IsAddedCalled { get; private set; }

        public static void Added(Item item)
        {
            IsAddedCalled = true;
            Assert.That(item, Is.Not.Null);
            Assert.That(item.Title, Is.EqualTo("Add_EventReceiver_Works_Test"));
        }
    }
}
