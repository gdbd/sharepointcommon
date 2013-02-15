using System;
using NUnit.Framework;
using SharepointCommon.Events;

namespace SharepointCommon.Test.Events
{
    public class ListEventsTest
    {
        private readonly string _webUrl = string.Format("http://{0}/", Environment.MachineName);

        [Test]
        public void Add_Remove_Receiever_Test()
        {
            using (var wf = WebFactory.Open(_webUrl))
            {
                IQueryList<Item> list = null;
                try
                {
                    list = wf.Create<Item>("Add_Remove_Receiever_Test");

                    list.Events.Add<ListEventReceiver>(er => er.ItemAdded);
                    list.Add(new Item { Title = "item1" });
                    Assert.That(ListEventReceiver.IsCalled);

                    list.Events.Remove<ListEventReceiver>(er => er.ItemAdded);
                    ListEventReceiver.IsCalled = false;
                    list.Add(new Item { Title = "item2" });
                    Assert.That(!ListEventReceiver.IsCalled);
                }
                finally
                {
                    if (list != null) list.DeleteList(false);
                }
            }
        }
    }
}
