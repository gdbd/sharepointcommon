using System;
using NUnit.Framework;
using SharepointCommon.Events;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.Events
{
    public class ListEventsTest
    {
        private readonly string _webUrl = string.Format("http://{0}/", Environment.MachineName);

        [Test]
        public void Add_Receiever_Test()
        {
            using (var wf = WebFactory.Open(_webUrl))
            {
                if (wf.ExistsByName("Add_Receiever_Test"))
                {
                    var l = wf.GetByName<Item>("Add_Receiever_Test");
                    l.DeleteList(false);
                }

                IQueryList<OneMoreField<string>> list = null;
                try
                {
                    list = wf.Create<OneMoreField<string>>("Add_Remove_Receiever_Test");
                   
                    list.AddEventReciver<ListEventReceiver>();
                    list.Add(new OneMoreField<string> { AdditionalField = "item1" });
                    Assert.That(ListEventReceiver.IsCalled);
                }
                finally
                {
                    if (list != null) list.DeleteList(false);
                }
            }
        }
    }
}
