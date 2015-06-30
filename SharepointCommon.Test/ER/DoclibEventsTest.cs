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
    public class DoclibEventsTest
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


        
        #region Document events
        [Test]
        public void Doc_Is_Added_Async_Called_Test()
        {
            using (var ts = new TestListScope<AddedDocAsync>("Is_Added_Async_Called_Test", true))
            {
                ts.List.AddEventReceiver<AddedDocReceiverAsync>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);

                AddedDocAsync.ManualResetEvent.WaitOne(10000);

                Assert.True(AddedDocAsync.IsAddCalled, "Not fired added receiver!");

                ValidateCustomItem(AddedDocAsync.Received, entity);

                if (AddedDocAsync.Exception != null)
                    throw AddedDocAsync.Exception;
            }
        }

        [Test]
        public void Doc_Is_Added_Sync_Called_Test()
        {
            using (var ts = new TestListScope<AddedDocSync>("Is_Added_Async_Called_Test", true))
            {
                ts.List.AddEventReceiver<AddedDocReceiverSync>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);

                AddedDocSync.ManualResetEvent.WaitOne(10000);

                Assert.True(AddedDocSync.IsAddCalled, "Not fired added receiver!");

                ValidateCustomItem(AddedDocSync.Received, entity);

                if (AddedDocSync.Exception != null)
                    throw AddedDocSync.Exception;
            }
        }

        [Test]
        public void Doc_Adding_Sync_Called_Test()
        {
            using (var ts = new TestListScope<AddingDoc>("Doc_Adding_Sync_Called_Test", true))
            {
                ts.List.AddEventReceiver<AddingDocReceiver>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);
                Assert.True(AddingDoc.IsAddCalled, "Not fired added receiver!");

                if (AddingItem.Exception != null)
                    throw AddingDoc.Exception;

                ValidateCustomItem(AddingDoc.Received, entity, true);
            }
        }

        [Test]
        public void Doc_Adding_Async_Throws_Test()
        {
            using (var ts = new TestListScope<AddingDocAsync>("Doc_Adding_Async_Throws_Test"))
            {
                Assert.Throws<SharepointCommonException>(() =>
                    ts.List.AddEventReceiver<AddingDocReceiverAsync>());
            }
        }

        [Test]
        public void Doc_Updated_Sync_Called_Test()
        {
            using (var ts = new TestListScope<UpdatedDoc>("Doc_Updated_Sync_Called_Test", true))
            {
                ts.List.AddEventReceiver<UpdatedDocReceiver>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);

                entity = ts.List.ById(entity.Id);

                ModifyCustomItem(entity);
                ts.List.Update(entity, true);
                Assert.True(UpdatedDoc.IsUpdateCalled, "Not fired updated receiver!");

                if (UpdatedDoc.Exception != null)
                    throw UpdatedDoc.Exception;

                ValidateCustomItem(UpdatedDoc.Recieved, entity);
            }
        }

        [Test]
        public void Doc_Updated_Async_Called_Test()
        {
            using (var ts = new TestListScope<UpdatedDocAsync>("Doc_Updated_Async_Called_Test", true))
            {
                ts.List.AddEventReceiver<UpdatedDocReceiverAsync>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);

                entity = ts.List.ById(entity.Id);

                ModifyCustomItem(entity);
                ts.List.Update(entity, true);

                UpdatedDocAsync.ManualResetEvent.WaitOne(10000);

                Assert.True(UpdatedDocAsync.IsUpdateCalled, "Not fired updated receiver!");

                if (UpdatedDocAsync.Exception != null)
                    throw UpdatedDocAsync.Exception;

                ValidateCustomItem(UpdatedDocAsync.Received, entity);
            }
        }

        [Test]
        public void Doc_Updating_Sync_Called_Test()
        {
            using (var ts = new TestListScope<UpdatingDoc>("Doc_Updating_Sync_Called_Test", true))
            {
                ts.List.AddEventReceiver<UpdatingDocReceiver>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);

                var origCopy = new UpdatingDoc(entity);

                entity = ts.List.ById(entity.Id);
                ModifyCustomItem(entity);
                ts.List.Update(entity, true);

                Assert.True(UpdatingDoc.IsUpdateCalled, "Not fired updating receiver!");

                if (UpdatingDoc.Exception != null)
                    throw UpdatingDoc.Exception;

                ValidateCustomItem(UpdatingDoc.ReceivedOrig, origCopy);
                ValidateCustomItem(UpdatingDoc.ReceivedChanged, entity, true);
            }
        }

        [Test]
        public void Doc_Updating_Async_Throws_Test()
        {
            using (var ts = new TestListScope<UpdatingDocAsync>("Is_Updating_Async_Called_Test"))
            {
                Assert.Throws<SharepointCommonException>(
                    () => ts.List.AddEventReceiver<UpdatingDocReceiverAsync>());
            }
        }

        [Test]
        public void Doc_Deleting_Sync_Called_Test()
        {
            using (var ts = new TestListScope<DeletingDoc>("Doc_Deleting_Sync_Called_Test", true))
            {
                ts.List.AddEventReceiver<DeletingDocReceiver>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);
                ts.List.Delete(entity, false);
                Assert.True(DeletingDoc.IsDeleteCalled, "Not fired added receiver!");

                if (DeletingItem.Exception != null)
                    throw DeletingDoc.Exception;

                ValidateCustomItem(DeletingDoc.Received, entity);
            }
        }

        [Test]
        public void Doc_Deleting_Async_Throws_Test()
        {
            using (var ts = new TestListScope<DeletingDocAsync>("Doc_Deleting_Async_Throws_Test"))
            {
                Assert.Throws<SharepointCommonException>(
                    () => ts.List.AddEventReceiver<DeletingDocReceiverAsync>());
            }
        }


        [Test]
        public void Doc_Deleted_Sync_Called_Test()
        {
            using (var ts = new TestListScope<DeletedDoc>("Doc_Deleted_Sync_Called_Test", true))
            {
                ts.List.AddEventReceiver<DeletedDocReceiver>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);
                ts.List.Delete(entity, false);
                Assert.True(DeletedDoc.IsDeleteCalled, "Not fired deleted receiver!");
                Assert.That(entity.Id, Is.EqualTo(DeletedDoc.DeletedId));

                if (DeletedDoc.Exception != null)
                    throw DeletedDoc.Exception;
            }
        }

        [Test]
        public void Doc_Deleted_Async_Called_Test()
        {
            using (var ts = new TestListScope<DeletedDocAsync>("Doc_Deleted_Async_Called_Test", true))
            {
                ts.List.AddEventReceiver<DeletedDocReceiverAsync>();
                var entity = FillCusomItem(ts);
                ts.List.Add(entity);
                ts.List.Delete(entity, false);

                DeletedDocAsync.ManualResetEvent.WaitOne(10000);

                Assert.True(DeletedDocAsync.IsDeleteCalled, "Not fired deleted receiver!");
                Assert.That(entity.Id, Is.EqualTo(DeletedDocAsync.DeletedId));

                if (DeletedDocAsync.Exception != null)
                    throw DeletedDocAsync.Exception;
            }
        }

        #endregion

        private T FillCusomItem<T>(TestListScope<T> ts) where T : Item, new()
        {
            var lookupItem = new Item { Title = ts.List.Title + "_lkp1" };
            ts.LookupList.Add(lookupItem);

            var lookupItem2 = new Item { Title = ts.List.Title + "_lkp2" };
            ts.LookupList.Add(lookupItem2);

            var nt = new T();

            if (nt is CustomItem)
            {
                var ci = nt as CustomItem;

                ci.Title = ts.List.Title;
                ci.CustomField1 = ts.List.Title + "_1";
                ci.CustomField2 = ts.List.Title + "_2";
                ci.CustomFieldNumber = 123.5;
                ci.CustomBoolean = true;
                ci.CustomUser = new Person(_firstUser.LoginName);
                ci.CustomUsers = new List<User> { new Person(_firstUser.LoginName), new User(_spGroup.Name) };
                ci.CustomLookup = lookupItem;
                ci.CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 };
                ci.CustomDate = DateTime.Now;
                ci.CustomChoice = TheChoice.Choice2;


                return (T)(Item)ci;
            }
            if (nt is CustomDocument)
            {
                var cd = nt as CustomDocument;

                cd.Title = ts.List.Title;
                cd.Name = ts.List.Title + ".bin";
                cd.Content = new byte[] { 10, 20, 48 };


                cd.CustomField1 = ts.List.Title + "_1";
                cd.CustomField2 = ts.List.Title + "_2";
                cd.CustomFieldNumber = 123.5;
                cd.CustomBoolean = true;
                cd.CustomUser = new Person(_firstUser.LoginName);
                cd.CustomUsers = new List<User> { new Person(_firstUser.LoginName), new User(_spGroup.Name) };
                cd.CustomLookup = lookupItem;
                cd.CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 };
                cd.CustomDate = DateTime.Now;
                cd.CustomChoice = TheChoice.Choice2;

                return (T)(Item)cd;
            }
            Assert.Fail();
            return null;
        }

        private void ValidateCustomItem<T>(T recieived, T orig, bool isAfterProprtyMapped = false) where T : Item
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

            if (recieived is CustomItem)
            {
                var ci = recieived as CustomItem;
                var co = orig as CustomItem;

                Assert.That(ci.Title, Is.EqualTo(orig.Title));
                Assert.That(ci.CustomField1, Is.EqualTo(co.CustomField1));
                Assert.That(ci.CustomField2, Is.EqualTo(co.CustomField2));
                Assert.That(ci.CustomFieldNumber, Is.EqualTo(co.CustomFieldNumber));
                Assert.That(ci.CustomBoolean, Is.EqualTo(co.CustomBoolean));
                Assert.That(ci.CustomUser.Id, Is.Not.EqualTo(0));

                Assert.That(ci.CustomUser.GetType().ToString(), Is.EqualTo("Castle.Proxies.PersonProxy"));


                Assert.That(ci.CustomUsers.Count(), Is.EqualTo(2));

                var users = ci.CustomUsers.ToList();
                Assert.That(users[0].GetType().ToString(), Is.EqualTo("Castle.Proxies.PersonProxy"));
                Assert.That(users[1].GetType().ToString(), Is.EqualTo("Castle.Proxies.UserProxy"));


                Assert.That(((Person)ci.CustomUsers.First()).Login, Is.EqualTo(((Person)co.CustomUsers.First()).Login));
                Assert.That(ci.CustomLookup, Is.Not.Null);
                Assert.That(ci.CustomLookup.Id, Is.EqualTo(co.CustomLookup.Id));
                Assert.That(ci.CustomMultiLookup, Is.Not.Null);
                Assert.That(ci.CustomMultiLookup.Count(), Is.EqualTo(2));
                Assert.That(ci.CustomMultiLookup.First().Title, Is.EqualTo(co.CustomMultiLookup.First().Title));
                Assert.That(ci.CustomChoice, Is.EqualTo(co.CustomChoice));
                Assert.That(ci.CustomDate.ToString(), Is.EqualTo(co.CustomDate.ToString()));
                Assert.That(ci.Тыдыщ, Is.EqualTo(co.Тыдыщ));
            }

            if (recieived is CustomDocument)
            {
                var ci = recieived as CustomDocument;
                var co = orig as CustomDocument;

                Assert.That(ci.Title, Is.EqualTo(orig.Title));
                Assert.That(ci.CustomField1, Is.EqualTo(co.CustomField1));
                Assert.That(ci.CustomField2, Is.EqualTo(co.CustomField2));
                Assert.That(ci.CustomFieldNumber, Is.EqualTo(co.CustomFieldNumber));
                Assert.That(ci.CustomBoolean, Is.EqualTo(co.CustomBoolean));
                Assert.That(ci.CustomUser.Id, Is.Not.EqualTo(0));

                Assert.That(ci.CustomUser.GetType().ToString(), Is.EqualTo("Castle.Proxies.PersonProxy"));


                Assert.That(ci.CustomUsers.Count(), Is.EqualTo(2));

                var users = ci.CustomUsers.ToList();
                Assert.That(users[0].GetType().ToString(), Is.EqualTo("Castle.Proxies.PersonProxy"));
                Assert.That(users[1].GetType().ToString(), Is.EqualTo("Castle.Proxies.UserProxy"));


                Assert.That(((Person)ci.CustomUsers.First()).Login, Is.EqualTo(((Person)co.CustomUsers.First()).Login));
                Assert.That(ci.CustomLookup, Is.Not.Null);
                Assert.That(ci.CustomLookup.Id, Is.EqualTo(co.CustomLookup.Id));
                Assert.That(ci.CustomMultiLookup, Is.Not.Null);
                Assert.That(ci.CustomMultiLookup.Count(), Is.EqualTo(2));
                Assert.That(ci.CustomMultiLookup.First().Title, Is.EqualTo(co.CustomMultiLookup.First().Title));
                Assert.That(ci.CustomChoice, Is.EqualTo(co.CustomChoice));
                Assert.That(ci.CustomDate.ToString(), Is.EqualTo(co.CustomDate.ToString()));
                Assert.That(ci.Тыдыщ, Is.EqualTo(co.Тыдыщ));
            }
        }

        private void ModifyCustomItem<T>(T entity) where T : Item
        {
            if (entity is CustomItem)
            {
                var ci = entity as CustomItem;
                ci.Title = "new title";

                ci.CustomField1 = "new CustomField1";
                ci.CustomField2 = "new CustomField2";
                ci.CustomFieldNumber = 778.1;
                ci.CustomBoolean = false;
                ci.CustomUser = new Person(_secondUser.LoginName);
                ci.CustomUsers = new List<User> { new Person(_secondUser.LoginName), new User(_spGroup.Name) };

                ci.CustomLookup = ci.CustomMultiLookup.Last();

                var old = ci.CustomMultiLookup.ToList();
                old.Reverse();

                ci.CustomMultiLookup = old;
                ci.CustomDate = DateTime.Now.AddDays(-2);
                ci.CustomChoice = TheChoice.Choice3;
            }

            else if (entity is CustomDocument)
            {
                var ci = entity as CustomDocument;
                ci.Title = "new title";

                ci.CustomField1 = "new CustomField1";
                ci.CustomField2 = "new CustomField2";
                ci.CustomFieldNumber = 778.1;
                ci.CustomBoolean = false;
                ci.CustomUser = new Person(_secondUser.LoginName);
                ci.CustomUsers = new List<User> { new Person(_secondUser.LoginName), new User(_spGroup.Name) };

                ci.CustomLookup = ci.CustomMultiLookup.Last();

                var old = ci.CustomMultiLookup.ToList();
                old.Reverse();

                ci.CustomMultiLookup = old;
                ci.CustomDate = DateTime.Now.AddDays(-2);
                ci.CustomChoice = TheChoice.Choice3;
            }
        }
    }
}
