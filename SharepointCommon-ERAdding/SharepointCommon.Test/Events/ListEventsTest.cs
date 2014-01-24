using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharepointCommon.Events;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.Events
{
    public class ListEventsTest
    {
        private readonly string _webUrl = string.Format("http://{0}:10000/", Environment.MachineName);

        [Test]
        public void Add_Receiever_Test()
        {
            using (var wf = WebFactory.Open(_webUrl))
            {
                if (wf.ExistsByName("Add_Remove_Receiever_Test"))
                {
                    var l = wf.GetByName<Item>("Add_Remove_Receiever_Test");
                    l.DeleteList(false);
                }

                IQueryList<OneMoreField<string>> list = null;
                try
                {
                    list = wf.Create<OneMoreField<string>>("Add_Remove_Receiever_Test");

                    list.AddEventReciver<TestListEventReceiver>();
                    list.Add(new OneMoreField<string> {AdditionalField = "item1", Title = "test1"});
                    //Thread.Sleep(5000);
                    var camlQuery =
                        Q.Where(Q.Eq(Q.FieldRef<OneMoreField<string>>(f => f.Title),
                            Q.Value("Text", "TestItem_Added")));
                    var items = list.Items(new CamlQuery().Query(camlQuery));
                    CollectionAssert.IsNotEmpty(items);
                    var item = items.FirstOrDefault();
                    if (item != null)
                    {
                        item.Title = "123";
                        item.ParentList.Update(item, false, i => i.Title);
                    }
                    camlQuery =
                        Q.Where(Q.Eq(Q.FieldRef<OneMoreField<string>>(f => f.Title),
                            Q.Value("Text", "ItemUpdated")));
                    items = list.Items(new CamlQuery().Query(camlQuery));
                    CollectionAssert.IsNotEmpty(items);

                    item.ParentList.Delete(item.Id, false);
                    //Assert.That(ListEventReceiver.IsCalled);
                }
                finally
                {
                    if (list != null) list.DeleteList(false);
                }
            }
        }
    }
}
