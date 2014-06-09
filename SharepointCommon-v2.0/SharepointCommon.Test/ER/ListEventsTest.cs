using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;
using NUnit.Framework;
using SharepointCommon.Test.Entity;
using SharepointCommon.Test.ER.Entities;
using SharepointCommon.Test.ER.Receivers;

namespace SharepointCommon.Test.ER
{
    public class ListEventsTest
    {
        private SPUser _firstUser;
        private SPGroup _spGroup;
        private SPUser _domainGroup;
        private SPUser _secondUser;

        [TestFixtureSetUp]
        public void Start()
        {
            var queryWeb = WebFactory.Open(Settings.TestSiteUrl);
           

            var users = queryWeb.Web.SiteUsers.Cast<SPUser>().ToList();
            var uu = users.Where(u => u.IsDomainGroup == false).ToList();

            _domainGroup = users.FirstOrDefault(u => u.IsDomainGroup);
            if (_domainGroup == null)
            {
                throw new Exception("No domain groups in site users!");
            }

            _spGroup = queryWeb.Web.SiteGroups[0];

            _firstUser = uu[0];
            _secondUser = uu[1];
        }


        [Test]
        public void Add_Remove_Receiever_Test()
        {
            using (var ts = new TestListScope<AddRemoveTest>("Add_Remove_Receiever_Test"))
            {
                ts.List.AddEventReciver<AddRemoveReceiver>();
                var entity = new AddRemoveTest { Title = "test1", TheText = "test2" };
                ts.List.Add(entity);
                Assert.True(AddRemoveTest.IsAddCalled, "Not fired added receiver!");

                ts.List.RemoveEventReciver<AddRemoveReceiver>();
                entity.Title = "asd-asd";
                ts.List.Update(entity, true);
                Assert.False(AddRemoveTest.IsUpdateCalled, "Fired after receiver was removed!");

                if (AddRemoveTest.Exception != null)
                    throw AddRemoveTest.Exception;
            }
        }

        #region Add, Adding

        [Test]
        public void Is_Added_Sync_Called_Test()
        {
            using (var ts = new TestListScope<AddedItem>("Is_Added_Sync_Called_Test", true))
            {
                ts.List.AddEventReciver<AddedReceiver>();

                var entity = FillCusomItem<AddedItem>(ts);
                entity.TheText = "test2";

                ts.List.Add(entity);
                Assert.True(AddedItem.IsAddCalled, "Not fired added receiver!");

                ValidateCustomItem(AddedItem.Received, entity);

                if (AddedItem.Exception != null)
                    throw AddedItem.Exception;
            }
        } 

        [Test]
        public void Is_Added_Async_Called_Test()
        {
            using (var ts = new TestListScope<AddedItemAsync>("Is_Added_Async_Called_Test", true))
            {
                ts.List.AddEventReciver<AddedReceiverAsync>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);

                AddedItemAsync.ManualResetEvent.WaitOne(10000);

                Assert.True(AddedItemAsync.IsAddCalled, "Not fired added receiver!");

                ValidateCustomItem(AddedItemAsync.Received, entity);

                if (AddedItemAsync.Exception != null)
                    throw AddedItemAsync.Exception;
            }
        }

        [Test]
        public void Is_Adding_Sync_Called_Test()
        {
            using (var ts = new TestListScope<AddingItem>("Is_Adding_Sync_Called_Test", true))
            {
                ts.List.AddEventReciver<AddingReceiver>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);
                Assert.True(AddingItem.IsAddCalled, "Not fired added receiver!");

                if (AddingItem.Exception != null)
                    throw AddingItem.Exception;

                ValidateCustomItem(AddingItem.Received, entity, true);
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
            using (var ts = new TestListScope<UpdatedItem>("Is_Updated_Sync_Called_Test", true))
            {
                ts.List.AddEventReciver<UpdatedReceiver>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);

                entity = ts.List.ById(entity.Id);

                ModifyCustomItem(entity);
                ts.List.Update(entity, true);
                Assert.True(UpdatedItem.IsUpdateCalled, "Not fired updated receiver!");

                if (UpdatedItem.Exception != null)
                    throw UpdatedItem.Exception;

                ValidateCustomItem(UpdatedItem.Recieved, entity);
            }
        }

        [Test]
        public void Is_Updated_Async_Called_Test()
        {
            using (var ts = new TestListScope<UpdatedItemAsync>("Is_Updated_Async_Called_Test", true))
            {
                ts.List.AddEventReciver<UpdatedReceiverAsync>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);

                entity = ts.List.ById(entity.Id);
                
                ModifyCustomItem(entity);
                ts.List.Update(entity, true);

                UpdatedItemAsync.ManualResetEvent.WaitOne(10000);

                Assert.True(UpdatedItemAsync.IsUpdateCalled, "Not fired updated receiver!");

                if (UpdatedItem.Exception != null)
                    throw UpdatedItemAsync.Exception;

                ValidateCustomItem(UpdatedItemAsync.Received, entity);
            }
        }

        [Test]
        public void Is_Updating_Sync_Called_Test()
        {
            using (var ts = new TestListScope<UpdatingItem>("Is_Updating_Sync_Called_Test", true))
            {
                ts.List.AddEventReciver<UpdatingReceiver>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);

                var origCopy = new UpdatingItem(entity);

                entity = ts.List.ById(entity.Id);
                ModifyCustomItem(entity);
                ts.List.Update(entity, true);

                Assert.True(UpdatingItem.IsUpdateCalled, "Not fired updating receiver!");

                if (UpdatingItem.Exception != null)
                    throw UpdatingItem.Exception;

                ValidateCustomItem(UpdatingItem.ReceivedOrig, origCopy);
                ValidateCustomItem(UpdatingItem.ReceivedChanged, entity, true);
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

                if (DeletedItem.Exception != null)
                    throw DeletedItem.Exception;
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

                if (DeletedItemAsync.Exception != null)
                    throw DeletedItemAsync.Exception;
            }
        }

        [Test]
        public void Is_Deleteing_Sync_Called_Test()
        {
            using (var ts = new TestListScope<DeletingItem>("Is_Deleteing_Sync_Called_Test", true))
            {
                ts.List.AddEventReciver<DeletingReceiver>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);
                ts.List.Delete(entity,false);
                Assert.True(DeletingItem.IsDeleteCalled, "Not fired added receiver!");

                if (DeletingItem.Exception != null)
                    throw DeletingItem.Exception;

                ValidateCustomItem(DeletingItem.Received, entity);
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
        
        private T FillCusomItem<T>(TestListScope<T> ts) where T : CustomItem, new()
        {
            var lookupItem = new Item { Title = ts.List.Title + "_lkp1" };
            ts.LookupList.Add(lookupItem);

            var lookupItem2 = new Item { Title = ts.List.Title + "_lkp2" };
            ts.LookupList.Add(lookupItem2);

            var ci = new T {
                Title = ts.List.Title,
                CustomField1 = ts.List.Title + "_1",
                CustomField2 = ts.List.Title + "_2",
                CustomFieldNumber = 123.5,
                CustomBoolean = true,
                CustomUser = new Person(_firstUser.LoginName),
                CustomUsers = new List<User> { new Person(_firstUser.LoginName), new User(_spGroup.Name) },
                CustomLookup = lookupItem,
                CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 },
                CustomDate = DateTime.Now,
                CustomChoice = TheChoice.Choice2,
            };

            return ci;
        }

        private void ValidateCustomItem<T>(T recieived, T orig, bool isAfterProprtyMapped = false) where T : CustomItem
        {
            Assert.IsNotNull(recieived);

            if (isAfterProprtyMapped)
            {
                Assert.That(recieived.Id, Is.EqualTo(0));
            }
            else
            {
                Assert.That(recieived.Id, Is.EqualTo(orig.Id));
            }

            
            Assert.That(recieived.Title, Is.EqualTo(orig.Title));
            Assert.That(recieived.CustomField1, Is.EqualTo(orig.CustomField1));
            Assert.That(recieived.CustomField2, Is.EqualTo(orig.CustomField2));
            Assert.That(recieived.CustomFieldNumber, Is.EqualTo(orig.CustomFieldNumber));
            Assert.That(recieived.CustomBoolean, Is.EqualTo(orig.CustomBoolean));
            Assert.That(recieived.CustomUser.Id, Is.Not.EqualTo(0));

            Assert.That(recieived.CustomUser.GetType().ToString(), Is.EqualTo("Castle.Proxies.PersonProxy"));

            var users = recieived.CustomUsers.ToList();
            Assert.That(users[0].GetType().ToString(), Is.EqualTo("Castle.Proxies.PersonProxy"));
            Assert.That(users[1].GetType().ToString(), Is.EqualTo("Castle.Proxies.UserProxy"));


            Assert.That(recieived.CustomUsers.Count(), Is.EqualTo(2));
            

            Assert.That(((Person)recieived.CustomUsers.First()).Login, Is.EqualTo(((Person)orig.CustomUsers.First()).Login));
            Assert.That(recieived.CustomLookup, Is.Not.Null);
            Assert.That(recieived.CustomLookup.Id, Is.EqualTo(orig.CustomLookup.Id));
            Assert.That(recieived.CustomMultiLookup, Is.Not.Null);
            Assert.That(recieived.CustomMultiLookup.Count(), Is.EqualTo(2));
            Assert.That(recieived.CustomMultiLookup.First().Title, Is.EqualTo(orig.CustomMultiLookup.First().Title));
            Assert.That(recieived.CustomChoice, Is.EqualTo(orig.CustomChoice));
            Assert.That(recieived.CustomDate.ToString(), Is.EqualTo(orig.CustomDate.ToString()));
            Assert.That(recieived.Тыдыщ, Is.EqualTo(orig.Тыдыщ));
        }

        private void ModifyCustomItem<T>(T entity) where T : CustomItem
        {
            entity.Title = "new title";

            entity.CustomField1 = "new CustomField1";
            entity.CustomField2 = "new CustomField2";
            entity.CustomFieldNumber = 778.1;
            entity.CustomBoolean = false;
            entity.CustomUser = new Person(_secondUser.LoginName);
            entity.CustomUsers = new List<User> { new Person(_secondUser.LoginName), new User(_spGroup.Name) };
         
            entity.CustomLookup = entity.CustomMultiLookup.Last();

            var old = entity.CustomMultiLookup.ToList();
            old.Reverse();

            entity.CustomMultiLookup = old;
            entity.CustomDate = DateTime.Now.AddDays(-2);
            entity.CustomChoice = TheChoice.Choice3;
        }
    }
}
