using NUnit.Framework;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class ListEventsTest
    {
        [Test]
        public void Add_Remove_Receiever_Test()
        {
            using (var ts = new TestListScope<AddRemoveTest>("Add_Remove_Receiever_Test"))
            {
                ts.List.AddEventReciver<AddRemoveTestEventReceiver>();
                var entity = new AddRemoveTest { Title = "test1", };
                ts.List.Add(entity);
                Assert.True(AddRemoveTest.IsAddCalled, "Not fired added receiver!");

                ts.List.RemoveEventReciver<AddRemoveTestEventReceiver>();
                entity.Title = "asd-asd";
                ts.List.Update(entity, true);
                Assert.False(AddRemoveTest.IsUpdateCalled, "Fired after receiver was removed!");
            }
        }

        #region Add, Adding

        [Test]
        public void Is_Added_Sync_Called_Test()
        {
            using (var ts = new TestListScope<AddedItem>("Is_Added_Sync_Called_Test"))
            {
                ts.List.AddEventReciver<AddedReceiver>();
                var entity = new AddedItem { Title = "test1", TheText = "test2" };
                ts.List.Add(entity);
                Assert.True(AddedItem.IsAddCalled, "Not fired added receiver!");
            }
        } 

        [Test]
        public void Is_Added_Async_Called_Test()
        {
            using (var ts = new TestListScope<AddedItemAsync>("Is_Added_Async_Called_Test"))
            {
                ts.List.AddEventReciver<AddedReceiverAsync>();
                var entity = new AddedItemAsync { Title = "test1", TheText = "test2" };
                ts.List.Add(entity);

                AddedItemAsync.ManualResetEvent.WaitOne(10000);

                Assert.True(AddedItemAsync.IsAddCalled, "Not fired added receiver!");
            }
        }

        [Test]
        public void Is_Adding_Sync_Called_Test()
        {
            using (var ts = new TestListScope<AddingItem>("Is_Adding_Sync_Called_Test"))
            {
                ts.List.AddEventReciver<AddingReceiver>();
                var entity = new AddingItem { Title = "test1", TheText = "test2" };
                ts.List.Add(entity);
                Assert.True(AddingItem.IsAddCalled, "Not fired added receiver!");
            }
        }

        [Test]
        public void Is_Adding_Async_Throws_Test()
        {
            using (var ts = new TestListScope<AddingItemAsync>("Is_Register_Adding_Async_Throws_Test"))
            {
                Assert.Throws<SharepointCommonException>(() =>
                    ts.List.AddEventReciver<AddingReceiverAsync>());
            }
        }

        #endregion

        #region Update, Updating

        [Test]
        public void Is_Updated_Sync_Called_Test()
        {
            using (var ts = new TestListScope<UpdatedItem>("Is_Updated_Sync_Called_Test"))
            {
                ts.List.AddEventReciver<UpdatedReceiver>();
                var entity = new UpdatedItem { Title = "test1", TheText = "test2" };
                ts.List.Add(entity);
                entity.Title = "new title";
                ts.List.Update(entity, true, i => i.Title);
                Assert.True(UpdatedItem.IsUpdateCalled, "Not fired updated receiver!");

                if (UpdatedItem.Exception != null)
                    throw UpdatedItem.Exception;
            }
        }

        [Test]
        public void Is_Updated_Async_Called_Test()
        {
            using (var ts = new TestListScope<UpdatedItemAsync>("Is_Updated_Async_Called_Test"))
            {
                ts.List.AddEventReciver<UpdatedReceiverAsync>();
                var entity = new UpdatedItemAsync { Title = "test1", TheText = "test2" };
                ts.List.Add(entity);
                entity.Title = "new title";
                ts.List.Update(entity, true, i => i.Title);

                UpdatedItemAsync.ManualResetEvent.WaitOne(10000);

                Assert.True(UpdatedItemAsync.IsUpdateCalled, "Not fired updated receiver!");

                if (UpdatedItem.Exception != null)
                    throw UpdatedItemAsync.Exception;
            }
        }

        [Test]
        public void Is_Updating_Sync_Called_Test()
        {
            using (var ts = new TestListScope<UpdatingItem>("Is_Updating_Sync_Called_Test"))
            {
                ts.List.AddEventReciver<UpdatingReceiver>();
                var entity = new UpdatingItem { Title = "test1", TheText = "test2" };
                ts.List.Add(entity);
                entity.Title = "new title";
                ts.List.Update(entity, true, i => i.Title);
                Assert.True(UpdatingItem.IsUpdateCalled, "Not fired updating receiver!");

                if (UpdatingItem.Exception != null)
                    throw UpdatingItem.Exception;
            }
        }

        [Test]
        public void Is_Updating_Async_Throws_Test()
        {
            using (var ts = new TestListScope<UpdatingItemAsync>("Is_Updating_Async_Called_Test"))
            {
                Assert.Throws<SharepointCommonException>(
                    () => ts.List.AddEventReciver<UpdatingReceiverAsync>());
            }
        }

        #endregion

        #region Delete, Deleting

        [Test]
        public void Is_Deleted_Sync_Called_Test()
        {
            using (var ts = new TestListScope<DeletedItem>("Is_Deleted_Sync_Called_Test"))
            {
                ts.List.AddEventReciver<DeletedReceiver>();
                var entity = new DeletedItem { Title = "test1", TheText = "test2" };
                ts.List.Add(entity);
                ts.List.Delete(entity, false);
                Assert.True(DeletedItem.IsDeleteCalled, "Not fired deleted receiver!");
                Assert.That(entity.Id, Is.EqualTo(DeletedItem.DeletedId));
            }
        }

        [Test]
        public void Is_Deleted_Async_Called_Test()
        {
            using (var ts = new TestListScope<DeletedItemAsync>("Is_Deleted_Async_Called_Test"))
            {
                ts.List.AddEventReciver<DeletedReceiverAsync>();
                var entity = new DeletedItemAsync { Title = "test1", TheText = "test2" };
                ts.List.Add(entity);
                ts.List.Delete(entity, false);

                DeletedItemAsync.ManualResetEvent.WaitOne(10000);

                Assert.True(DeletedItemAsync.IsDeleteCalled, "Not fired deleted receiver!");
                Assert.That(entity.Id, Is.EqualTo(DeletedItemAsync.DeletedId));
            }
        }

        [Test]
        public void Is_Deleteing_Sync_Called_Test()
        {
            using (var ts = new TestListScope<DeletingItem>("Is_Deleteing_Sync_Called_Test"))
            {
                ts.List.AddEventReciver<DeletingReceiver>();
                var entity = new DeletingItem { Title = "test1", TheText = "test2" };
                ts.List.Add(entity);
                ts.List.Delete(entity,false);
                Assert.True(DeletingItem.IsDeleteCalled, "Not fired added receiver!");
            }
        }

        [Test]
        public void Is_Deleting_Async_Throws_Test()
        {
            using (var ts = new TestListScope<DeletingItemAsync>("Is_Deleting_Async_Throws_Test"))
            {
                Assert.Throws<SharepointCommonException>(
                    () => ts.List.AddEventReciver<DeletingReceiverAsync>());
            }
        }

        #endregion
    }
}
