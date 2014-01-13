using SharepointCommon.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.SharePoint;
    using NUnit.Framework;
using SharepointCommon.Test.Entity;
    using Assert = NUnit.Framework.Assert;

namespace SharepointCommon.Test
{


    [TestFixture]
    public class QueryListTests
    {
        private const string ListName1 = "SharepointCommonTestList";
        private const string ListForLookup = "ListForLookup";

        private readonly string _webUrl = Settings.TestSiteUrl;

        private SPUser _firstUser;
        private SPUser _secondUser;
        private SPUser _domainGroup;
        private SPGroup _spGroup;
        
        private IQueryList<Item> _list;
        private IQueryList<Item> _listForLookup;
        private IQueryWeb _queryWeb;
        
        [TestFixtureSetUp]
        public void Start()
        {
            _queryWeb = WebFactory.Open(_webUrl);
            try
            {
                _list = _queryWeb.Create<Item>(ListName1);
            }
            catch (SPException)
            {
                // if tests aborted, list exist from previous session
                _list = _queryWeb.GetByName<Item>(ListName1);
                _list.DeleteList(false);
                _list = _queryWeb.Create<Item>(ListName1);
            }

            try
            {
                _listForLookup = _queryWeb.Create<Item>(ListForLookup);
            }
            catch (SPException)
            {
                // if tests aborted, list exist from previous session
                _listForLookup = _queryWeb.GetByName<Item>(ListForLookup);
                _listForLookup.DeleteList(false);
                _listForLookup = _queryWeb.Create<Item>(ListForLookup);
            }

            var users = _queryWeb.Web.SiteUsers.Cast<SPUser>().ToList();
            var uu = users.Where(u => u.IsDomainGroup == false).ToList();

            _domainGroup = users.FirstOrDefault(u => u.IsDomainGroup);
            if (_domainGroup == null)
            {
                throw new Exception("No domain groups in site users!");
            }

            _spGroup = _queryWeb.Web.SiteGroups[0];

            _firstUser = uu[0];
            _secondUser = uu[1];
        }

        [TestFixtureTearDown]
        public void Stop()
        {
            _list.DeleteList(false);
            _listForLookup.DeleteList(false);
            _queryWeb.Dispose();
        }

        #region  Add Item Tests
        [Test]
        public void Add_Adds_Item_Test()
        {
            var item = new Item { Title = "Add_AddsItemTest" };
            _list.Add(item);
            Assert.That(item.Id, Is.Not.EqualTo(0));

            var items = _list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef<Item>(i => i.Title), Q.Value("Add_AddsItemTest")))))
                .ToList();

            CollectionAssert.IsNotEmpty(items);
            Assert.That(items.First().Id, Is.EqualTo(item.Id));
            Assert.That(items.First().Title, Is.EqualTo("Add_AddsItemTest"));
        }

        [Test]
        public void Add_Adds_CustomItem()
        {
            IQueryList<CustomItem> list = null;
            try
            {
                var lookupItem = new Item { Title = "Add_Adds_CustomItem_Test_Lookup" };
                _listForLookup.Add(lookupItem);

                var lookupItem2 = new Item { Title = "Add_Adds_CustomItem_Test_Lookup_2" };
                _listForLookup.Add(lookupItem2);

                list = _queryWeb.Create<CustomItem>("Add_AddsCustomItem");
                var customItem = new CustomItem
                    {
                        Title = "Items_ReturnsColectionOfCustomItemsTest",
                        CustomField1 = "Items_ReturnsColectionOfCustomItemsTest1",
                        CustomField2 = "Items_ReturnsColectionOfCustomItemsTest2",
                        CustomFieldNumber = 123.5,
                        CustomBoolean = true,
                        CustomUser = new Person(_firstUser.LoginName),
                        CustomUsers = new List<User> { new Person(_firstUser.LoginName), new User(_spGroup.Name) },
                        CustomLookup = lookupItem,
                        CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 },
                        CustomDate = DateTime.Now,
                        CustomChoice = TheChoice.Choice2,
                        Тыдыщ = "тест",
                    };
                list.Add(customItem);

                var item = list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef<CustomItem>(i => i.Title), Q.Value("Items_ReturnsColectionOfCustomItemsTest")))))
                .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(customItem.Id));
                Assert.That(item.Title, Is.EqualTo(customItem.Title));
                Assert.That(item.CustomField1, Is.EqualTo(customItem.CustomField1));
                Assert.That(item.CustomField2, Is.EqualTo(customItem.CustomField2));
                Assert.That(item.CustomFieldNumber, Is.EqualTo(customItem.CustomFieldNumber));
                Assert.That(item.CustomBoolean, Is.EqualTo(customItem.CustomBoolean));
                Assert.That(item.CustomUser.Id, Is.Not.EqualTo(0));

                Assert.That(item.CustomUser.GetType().ToString(), Is.EqualTo("Castle.Proxies.PersonProxy"));

                var users = item.CustomUsers.ToList();
                Assert.That(users[0].GetType().ToString(), Is.EqualTo("Castle.Proxies.PersonProxy"));
                Assert.That(users[1].GetType().ToString(), Is.EqualTo("Castle.Proxies.UserProxy"));


                Assert.That(item.CustomUsers.Count(), Is.EqualTo(2));
                Assert.That(item.CustomUsers.First().Id, Is.EqualTo(_firstUser.ID));
                Assert.That(item.CustomLookup, Is.Not.Null);
                Assert.That(item.CustomLookup.Id, Is.EqualTo(lookupItem.Id));
                Assert.That(item.CustomMultiLookup, Is.Not.Null);
                Assert.That(item.CustomMultiLookup.Count(), Is.EqualTo(2));
                Assert.That(item.CustomMultiLookup.First().Title, Is.EqualTo(lookupItem.Title));
                Assert.That(item.CustomChoice, Is.EqualTo(customItem.CustomChoice));
                Assert.That(item.Тыдыщ, Is.EqualTo(customItem.Тыдыщ));
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Ensure_Lookup_Sets_ShowField_Test()
        {
            IQueryList<LookupWithShowField> list = null;
            try
            {
                list = _queryWeb.Create<LookupWithShowField>("Ensure_Lookup_Sets_ShowField_Test");

                var field = list.GetField(a => a.CustomLookup);
                var field2 = list.GetField(a => a.CustomLookupWithShowField);

                Assert.That(field.LookupField, Is.EqualTo("Title"));
                Assert.That(field2.LookupField, Is.EqualTo("ID"));
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }


        [Test]
        public void Add_Adds_CustomItem_With_Empty_Fields()
        {
            IQueryList<CustomItem> list = null;
            try
            {
                list = _queryWeb.Create<CustomItem>("Add_Adds_CustomItem_With_Empty_Fields");
                var customItem = new CustomItem
                {
                    Title = null,
                    CustomField1 = null,
                    CustomField2 = null,
                   // CustomFieldNumber = 123.5,
                   // CustomBoolean = true,
                    CustomUser = null,
                    CustomUsers = null,
                    CustomLookup = null,
                    CustomMultiLookup = null,
                    CustomDate = null,
                  //  CustomChoice = TheChoice.Choice2,
                    Тыдыщ = null,
                };
                list.Add(customItem);

                var item = list.ById(customItem.Id);

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(customItem.Id));
                Assert.That(item.Title, Is.EqualTo(customItem.Title));
                Assert.That(item.CustomField1, Is.EqualTo(customItem.CustomField1));
                Assert.That(item.CustomField2, Is.EqualTo(customItem.CustomField2));
                Assert.That(item.CustomFieldNumber, Is.EqualTo(customItem.CustomFieldNumber));
                Assert.That(item.CustomBoolean, Is.EqualTo(customItem.CustomBoolean));
              
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Add_Adds_Item_To_Folder_Test()
        {
            IQueryList<Item> list = null;
            try
            {
                list = _queryWeb.Create<Item>("Add_Adds_Item_To_Folder_Test");
                var itm = new Item
                {
                    Title = "Add_Adds_Item_To_Folder_Test",
                    Folder = "Folder1/Folder2/Folder3",
                };
                list.Add(itm);

                var item = list.Items(new CamlQuery()
                    .Recursive()
                    //  .Folder(document.Url)
                    .Query(Q.Where(Q.Eq(Q.FieldRef<Item>(d => d.Title), Q.Value(itm.Title)))))
                    .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(itm.Id));
                Assert.That(item.Folder, Is.EqualTo(itm.Folder));
                Assert.That(item.Title, Is.EqualTo(itm.Title));
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Add_Throws_On_Property_No_Virtual()
        {
            IQueryList<CustomItemNoVirtualProperty> list = null;
            try
            {
                TestDelegate work = () =>
                    {
                        var lookupItem = new Item { Title = "Add_Adds_CustomItem_Test_Lookup" };
                        _listForLookup.Add(lookupItem);

                        var lookupItem2 = new Item { Title = "Add_Adds_CustomItem_Test_Lookup_2" };
                        _listForLookup.Add(lookupItem2);

                        list = _queryWeb.Create<CustomItemNoVirtualProperty>("Add_AddsCustomItem");
                        var customItem = new CustomItemNoVirtualProperty()
                            {
                                Title = "Items_ReturnsColectionOfCustomItemsTest",
                                CustomField1 = "Items_ReturnsColectionOfCustomItemsTest1",
                                CustomField2 = "Items_ReturnsColectionOfCustomItemsTest2",
                                CustomFieldNumber = 123.5,
                                CustomBoolean = true,
                                CustomUser = new Person(_firstUser.LoginName),
                                CustomUsers = new List<User> { new Person(_firstUser.LoginName) },
                                CustomLookup = lookupItem,
                                CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 }
                            };
                        list.Add(customItem);
                    };
                var exc = Assert.Throws<SharepointCommonException>(work);
                Assert.That(exc.Message, Is.EqualTo("Property CustomField1 must be virtual to work correctly."));
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Add_Throws_OnUserCollectionNoVirtual()
        {
            IQueryList<CustomItemNoVirtualProperty> list = null;
            try
            {
                TestDelegate work = () =>
                {
                    var lookupItem = new Item { Title = "Add_Adds_CustomItem_Test_Lookup" };
                    _listForLookup.Add(lookupItem);

                    var lookupItem2 = new Item { Title = "Add_Adds_CustomItem_Test_Lookup_2" };
                    _listForLookup.Add(lookupItem2);

                    list = _queryWeb.Create<CustomItemNoVirtualProperty>("Add_AddsCustomItem");
                    var customItem = new CustomItemNoVirtualProperty()
                    {
                        Title = "Items_ReturnsColectionOfCustomItemsTest",
                        CustomField1 = "Items_ReturnsColectionOfCustomItemsTest1",
                        CustomField2 = "Items_ReturnsColectionOfCustomItemsTest2",
                        CustomFieldNumber = 123.5,
                        CustomBoolean = true,
                        CustomUser = new Person(_firstUser.LoginName),
                        CustomUsers = new List<User> { new Person(_firstUser.LoginName), },
                        CustomLookup = lookupItem,
                        CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 }
                    };
                    list.Add(customItem);
                };
                var exc = Assert.Throws<SharepointCommonException>(work);
                Assert.That(exc.Message, Is.EqualTo("Property CustomField1 must be virtual to work correctly."));
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Add_Adds_Item_Of_ContentType_Test()
        {
            _list.AddContentType<Announcement>();
            var announcement = new Announcement
            {
                Title = "Add_AddsItemOfContentTypeTest",
                Body = "Add_AddsItemOfContentTypeTest",
                Expires = DateTime.Now
            };
            _list.Add(announcement);

            var item = _list.Items<Announcement>(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef<Announcement>(a => a.Title), Q.Value("Add_AddsItemOfContentTypeTest")))))
                .FirstOrDefault();

            Assert.IsNotNull(item);
            Assert.That(item.Id, Is.EqualTo(announcement.Id));
            Assert.That(item.Title, Is.EqualTo("Add_AddsItemOfContentTypeTest"));
            Assert.That(item.Body, Is.EqualTo("Add_AddsItemOfContentTypeTest"));
        }

        [Test]
        public void Add_Adds_Item_With_Field_Attribute_Test()
        {
            _list.AddContentType<Holiday>();
            var holiday = new Holiday
            {
                Title = "Items_ReturnsOfSpecificContentType",
                Category = Category.Category1,
                IsNonWorkingDay = true,
                Date = DateTime.Now
            };
            _list.Add(holiday);
            Assert.That(holiday.Id, Is.Not.EqualTo(0));
        }

        [Test]
        public void Add_Adds_CustomItem_With_Nullable()
        {
            using (var factory = WebFactory.Open(_webUrl))
            {
                IQueryList<NullableItem> list = null;
                try
                {
                    list = factory.Create<NullableItem>("Add_Adds_CustomItem_With_Nullable");

                    var item = new NullableItem();
                    list.Add(item);

                    var item2 = list.ById(item.Id);

                    Assert.Null(item2.CustomDouble);
                    Assert.Null(item2.CustomInt);
                    Assert.Null(item2.CustomDecimal);
                    Assert.Null(item2.CustomBoolean);
                    Assert.Null(item2.CustomDate);
                    Assert.Null(item2.CustomChoice);
                }
                finally
                {
                    if (list != null)
                    {
                        list.DeleteList(false);
                    }
                }
            }
        }

        [Test]
        public void Add_Adds_CustomItem_With_Nullable_Set_Fields()
        {
            using (var factory = WebFactory.Open(_webUrl))
            {
                IQueryList<NullableItem> list = null;
                try
                {
                    list = factory.Create<NullableItem>("Add_Adds_CustomItem_With_Nullable");

                    var item = new NullableItem
                                   {
                                       CustomBoolean = true,
                                       CustomDouble = 123.5,
                                       CustomDate = DateTime.Now,
                                       CustomInt = 101,
                                       CustomDecimal = 15000,
                                       CustomChoice = TheChoice.Choice2,
                                   };
                    list.Add(item);

                    var item2 = list.ById(item.Id);

                    Assert.That(item2.CustomBoolean, Is.True);
                    Assert.That(item2.CustomDouble, Is.EqualTo(item.CustomDouble));
                    Assert.That(item2.CustomDate, Is.EqualTo(item.CustomDate).Within(1).Minutes);
                    Assert.That(item2.CustomInt, Is.EqualTo(item.CustomInt));
                    Assert.That(item2.CustomDecimal, Is.EqualTo(item.CustomDecimal));
                    Assert.That(item2.CustomChoice, Is.EqualTo(item.CustomChoice));
                }
                finally
                {
                    if (list != null)
                    {
                        list.DeleteList(false);
                    }
                }
            }
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_Lookup_Updates_Item_Test()
        {
            IQueryList<CustomItem> list = null;
            try
            {
                var lookupItem = new Item { Title = "lkp1" };
                _listForLookup.Add(lookupItem);

                var lookupItem2 = new Item { Title = "lkp2" };
                _listForLookup.Add(lookupItem2);

                if (_queryWeb.ExistsByName("Update_Lookup_Updates_Item_Test"))
                    _queryWeb.Web.Lists["Update_Lookup_Updates_Item_Test"].Delete();

                list = _queryWeb.Create<CustomItem>("Update_Lookup_Updates_Item_Test");
                

                var customItem = new CustomItem
                {
                    Title = "val1",
                    CustomField1 = "val2",
                    CustomField2 = "val3",
                    CustomFieldNumber = 123.5,
                    CustomBoolean = true,
                    CustomUser = new Person(_firstUser.LoginName),
                    CustomUsers = new List<User> { new Person(_firstUser.LoginName), },
                    CustomLookup = lookupItem,
                    CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 },
                    CustomDate = DateTime.Now,
                    Тыдыщ = "тест",
                };
                list.Add(customItem);

                customItem = list.ById(customItem.Id);

                customItem.CustomLookup.Title = "test";

                var title = customItem.CustomLookup.Title;

            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Update_By_Field_Selector_Updates_Item_Test()
        {
            var item = new Item { Title = "Update_By_Field_Selector_Updates_Item_Test" };
            _list.Add(item);
            Assert.That(item.Id, Is.Not.EqualTo(0));

            var item2 = _list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Update_By_Field_Selector_Updates_Item_Test"))))).FirstOrDefault();

            Assert.That(item2, Is.Not.Null);
            Assert.That(item2.Id, Is.EqualTo(item.Id));
            Assert.That(item2.Title, Is.EqualTo("Update_By_Field_Selector_Updates_Item_Test"));

            item2.Title = "Update_By_Field_Selector_Updates_Item_Test_Updated";
            _list.Update(item2, true, i => i.Title);

            var item3 = _list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Update_By_Field_Selector_Updates_Item_Test_Updated")))))
                .FirstOrDefault();

            Assert.That(item3, Is.Not.Null);
            Assert.That(item3.Id, Is.EqualTo(item.Id));
            Assert.That(item3.Title, Is.EqualTo("Update_By_Field_Selector_Updates_Item_Test_Updated"));
        }

        [Test]
        public void Update_By_Field_Selector_Updates_CustomItem_Test()
        {
            IQueryList<CustomItem> list = null;
            try
            {
                var lookupItem = new Item { Title = "lkp1" };
                _listForLookup.Add(lookupItem);

                var lookupItem2 = new Item { Title = "lkp2" };
                _listForLookup.Add(lookupItem2);

                list = _queryWeb.Create<CustomItem>("Update_By_Field_Selector_Updates_CustomItem_Test");
                var customItem = new CustomItem
                {
                    Title = "val1",
                    CustomField1 = "val2",
                    CustomField2 = "val3",
                    CustomFieldNumber = 123.5,
                    CustomBoolean = true,
                    CustomUser = new Person(_firstUser.LoginName),
                    CustomUsers = new List<User> { new Person(_firstUser.LoginName), },
                    CustomLookup = lookupItem,
                    CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 },
                    CustomDate = DateTime.Now,
                    Тыдыщ = "тест",
                };
                list.Add(customItem);

                customItem.Title = "val1_";
                customItem.CustomField1 = "val2_";
                customItem.CustomField2 = "val3_";
                customItem.CustomFieldNumber = 235;
                customItem.CustomBoolean = false;
                customItem.CustomUser = new Person(_secondUser.LoginName);
                customItem.CustomUsers = new List<User>
                    {
                        new Person(_firstUser.LoginName),
                        new Person(_secondUser.LoginName)
                    };
                customItem.CustomLookup = lookupItem2;
                customItem.CustomMultiLookup = new List<Item> { lookupItem2 };
                customItem.Тыдыщ = "обновлено";

                list.Update(customItem, 
                    true, 
                    c => c.Title, 
                    c => c.CustomField1, 
                    c => c.CustomField2,
                    c => c.CustomFieldNumber,
                    c => c.CustomBoolean,
                    c => c.CustomUser,
                    c => c.CustomUsers,
                    c => c.CustomLookup,
                    c => c.CustomMultiLookup,
                    c => c.Тыдыщ);

                var item = list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef<Item>(i => i.Title), Q.Value("val1_")))))
                .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(customItem.Id));
                Assert.That(item.Title, Is.EqualTo(customItem.Title));
                Assert.That(item.CustomField1, Is.EqualTo(customItem.CustomField1));
                Assert.That(item.Тыдыщ, Is.EqualTo(customItem.Тыдыщ));
                Assert.That(item.CustomField2, Is.EqualTo(customItem.CustomField2));
                Assert.That(item.CustomFieldNumber, Is.EqualTo(customItem.CustomFieldNumber));
                Assert.That(item.CustomBoolean, Is.EqualTo(customItem.CustomBoolean));
                Assert.That(item.CustomUser, Is.Not.Null);
                Assert.That(item.CustomUser.Id, Is.Not.EqualTo(0));
                Assert.That(item.CustomUsers.Count(), Is.EqualTo(2));
                Assert.That(item.CustomUsers.First().Id, Is.EqualTo(_firstUser.ID));
                Assert.That(item.CustomLookup, Is.Not.Null);
                Assert.That(item.CustomLookup.Id, Is.EqualTo(lookupItem2.Id));
                Assert.That(item.CustomMultiLookup, Is.Not.Null);
                Assert.That(item.CustomMultiLookup.Count(), Is.EqualTo(1));
                Assert.That(item.CustomMultiLookup.First().Title, Is.EqualTo(lookupItem2.Title));
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Update_Updates_Item_Test()
        {
            IQueryList<Item> list = null;
            try
            {
                list = _queryWeb.Create<Item>("Update_Updates_Item_Test");
                list.IsVersioningEnabled = true;

                var item = new Item { Title = "Update_Whole_Item_Updates_Item_Test" };
                list.Add(item);
                Assert.That(item.Id, Is.Not.EqualTo(0));

                var item2 = list.Items(new CamlQuery()
                    .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Update_Whole_Item_Updates_Item_Test"))))).FirstOrDefault();

                Assert.That(item2, Is.Not.Null);
                Assert.That(item2.Id, Is.EqualTo(item.Id));
                Assert.That(item2.Title, Is.EqualTo("Update_Whole_Item_Updates_Item_Test"));

                item2.Title = "Update_Whole_Item_Updates_Item_Test_Updated";
                list.Update(item2, true);

                var item3 = list.Items(new CamlQuery()
                    .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Update_Whole_Item_Updates_Item_Test_Updated")))))
                    .FirstOrDefault();

                Assert.That(item3, Is.Not.Null);
                Assert.That(item3.Id, Is.EqualTo(item.Id));
                Assert.That(item3.Title, Is.EqualTo("Update_Whole_Item_Updates_Item_Test_Updated"));
                Assert.That(item3.Version.Major, Is.EqualTo(2));
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }

        [Test]
        public void Update_Updates_CustomItem_Test()
        {
            IQueryList<CustomItem> list = null;
            try
            {
                var lookupItem = new Item { Title = "Update_Updates_CustomItem_Test_Lookup" };
                _listForLookup.Add(lookupItem);

                var lookupItem2 = new Item { Title = "Update_Updates_CustomItem_Test_2" };
                _listForLookup.Add(lookupItem2);

                list = _queryWeb.Create<CustomItem>("Update_Updates_CustomItem_Test");
                var customItem = new CustomItem
                {
                    Title = "Update_Updates_CustomItem_Test",
                    CustomField1 = "Update_Updates_CustomItem_Test1",
                    CustomField2 = "Update_Updates_CustomItem_Test2",
                    CustomFieldNumber = 123.5,
                    CustomBoolean = true,
                    CustomUser = new Person(_firstUser.LoginName),
                    CustomUsers = new List<User> { new Person(_firstUser.LoginName), },
                    CustomLookup = lookupItem,
                    CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 },
                    CustomDate = DateTime.Now,
                    Тыдыщ = "тест",
                };
                list.Add(customItem);

                customItem.Title = "Update_Updates_CustomItem_Test_Updated";
                customItem.CustomField1 = "Update_Updates_CustomItem_Test1";
                customItem.CustomField2 = "Update_Updates_CustomItem_Test2";
                customItem.CustomFieldNumber = 235;
                customItem.CustomBoolean = false;
                customItem.CustomUser = new Person(_secondUser.LoginName);
                customItem.CustomUsers = new List<User>
                    {
                        new Person(_firstUser.LoginName),
                        new Person(_secondUser.LoginName)
                    };
                customItem.CustomLookup = lookupItem2;
                customItem.CustomMultiLookup = new List<Item> { lookupItem2 };
                customItem.Тыдыщ = "обновлено";

                list.Update(customItem, true);

                var item = list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Update_Updates_CustomItem_Test_Updated")))))
                .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(customItem.Id));
                Assert.That(item.Title, Is.EqualTo(customItem.Title));
                Assert.That(item.CustomField1, Is.EqualTo(customItem.CustomField1));
                Assert.That(item.Тыдыщ, Is.EqualTo(customItem.Тыдыщ));
                Assert.That(item.CustomField2, Is.EqualTo(customItem.CustomField2));
                Assert.That(item.CustomFieldNumber, Is.EqualTo(customItem.CustomFieldNumber));
                Assert.That(item.CustomBoolean, Is.EqualTo(customItem.CustomBoolean));
                Assert.That(item.CustomUser, Is.Not.Null);
                Assert.That(item.CustomUser.Id, Is.Not.EqualTo(0));
                Assert.That(item.CustomUsers.Count(), Is.EqualTo(2));
                Assert.That(item.CustomUsers.First().Id, Is.EqualTo(_firstUser.ID));
                Assert.That(item.CustomLookup, Is.Not.Null);
                Assert.That(item.CustomLookup.Id, Is.EqualTo(lookupItem2.Id));
                Assert.That(item.CustomMultiLookup, Is.Not.Null);
                Assert.That(item.CustomMultiLookup.Count(), Is.EqualTo(1));
                Assert.That(item.CustomMultiLookup.First().Title, Is.EqualTo(lookupItem2.Title));
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }
        
        [Test]
        public void Update_Updates_CustomItem_GetById_Test()
        {
            IQueryList<CustomItem> list = null;
            try
            {
                var lookupItem = new Item { Title = "Update_Updates_CustomItem_Test_Lookup" };
                _listForLookup.Add(lookupItem);

                var lookupItem2 = new Item { Title = "Update_Updates_CustomItem_Test_2" };
                _listForLookup.Add(lookupItem2);

                list = _queryWeb.Create<CustomItem>("Update_Updates_CustomItem_Test");
                var customItem = new CustomItem
                {
                    Title = "Update_Updates_CustomItem_Test",
                    CustomField1 = "Update_Updates_CustomItem_Test1",
                    CustomField2 = "Update_Updates_CustomItem_Test2",
                    CustomFieldNumber = 123.5,
                    CustomBoolean = true,
                    CustomUser = new Person(_firstUser.LoginName),
                    CustomUsers = new List<User> { new Person(_firstUser.LoginName), },
                    CustomLookup = lookupItem,
                    CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 },
                    CustomDate = DateTime.Now,
                    Тыдыщ = "тест",
                };
                list.Add(customItem);

                customItem = list.ById(customItem.Id);

                customItem.Title = "Update_Updates_CustomItem_Test_Updated";
                customItem.CustomField1 = "Update_Updates_CustomItem_Test1";
                customItem.CustomField2 = "Update_Updates_CustomItem_Test2";
                customItem.CustomFieldNumber = 235;
                customItem.CustomBoolean = false;
                customItem.CustomUser = new Person(_secondUser.LoginName);
                customItem.CustomUsers = new List<User>
                    {
                        new Person(_firstUser.LoginName),
                        new Person(_secondUser.LoginName)
                    };
                customItem.CustomLookup = lookupItem2;
                customItem.CustomMultiLookup = new List<Item> { lookupItem2 };
                customItem.Тыдыщ = "обновлено";

                list.Update(customItem, true);

                var item = list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Update_Updates_CustomItem_Test_Updated")))))
                .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(customItem.Id));
                Assert.That(item.Title, Is.EqualTo(customItem.Title));
                Assert.That(item.CustomField1, Is.EqualTo(customItem.CustomField1));
                Assert.That(item.Тыдыщ, Is.EqualTo(customItem.Тыдыщ));
                Assert.That(item.CustomField2, Is.EqualTo(customItem.CustomField2));
                Assert.That(item.CustomFieldNumber, Is.EqualTo(customItem.CustomFieldNumber));
                Assert.That(item.CustomBoolean, Is.EqualTo(customItem.CustomBoolean));
                Assert.That(item.CustomUser, Is.Not.Null);
                Assert.That(item.CustomUser.Id, Is.Not.EqualTo(0));
                Assert.That(item.CustomUsers.Count(), Is.EqualTo(2));
                Assert.That(item.CustomUsers.First().Id, Is.EqualTo(_firstUser.ID));
                Assert.That(item.CustomLookup, Is.Not.Null);
                Assert.That(item.CustomLookup.Id, Is.EqualTo(lookupItem2.Id));
                Assert.That(item.CustomMultiLookup, Is.Not.Null);
                Assert.That(item.CustomMultiLookup.Count(), Is.EqualTo(1));
                Assert.That(item.CustomMultiLookup.First().Title, Is.EqualTo(lookupItem2.Title));
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Update_Updates_Field_By_Null_Test()
        {
            IQueryList<CustomItem> list = null;
            try
            {
                var lookupItem = new Item { Title = "Update_Updates_Field_By_Null_Test1" };
                _listForLookup.Add(lookupItem);

                var lookupItem2 = new Item { Title = "Update_Updates_Field_By_Null_Test2" };
                _listForLookup.Add(lookupItem2);

                list = _queryWeb.Create<CustomItem>("Update_Updates_Field_By_Null_Test");
                var customItem = new CustomItem
                {
                    Title = "Update_Updates_CustomItem_Test",
                    CustomField1 = "Update_Updates_CustomItem_Test1",
                    CustomField2 = "Update_Updates_CustomItem_Test2",
                    CustomFieldNumber = 123.5,
                    CustomBoolean = true,
                    CustomUser = new Person(_firstUser.LoginName),
                    CustomUsers = new List<User> { new Person(_firstUser.LoginName) },
                    CustomLookup = lookupItem,
                    CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 },
                    CustomDate = DateTime.Now,
                };
                list.Add(customItem);

                customItem.Title = null;
                customItem.CustomField1 = null;
                customItem.CustomField2 = null;
                customItem.CustomFieldNumber = default(double);
                customItem.CustomBoolean = false;
                customItem.CustomUser = null;
                customItem.CustomUsers = null;
                customItem.CustomLookup = null;
                customItem.CustomMultiLookup = null;
                customItem.CustomDate = null;

                list.Update(customItem, true);

                var item = list.ById(customItem.Id);

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(customItem.Id));
                Assert.That(item.Title, Is.EqualTo(customItem.Title));
                Assert.That(item.CustomField1, Is.EqualTo(customItem.CustomField1));
                Assert.That(item.CustomField2, Is.EqualTo(customItem.CustomField2));
                Assert.That(item.CustomFieldNumber, Is.EqualTo(customItem.CustomFieldNumber));
                Assert.That(item.CustomBoolean, Is.EqualTo(customItem.CustomBoolean));
                Assert.That(item.CustomUser, Is.Null);
                Assert.That(item.CustomUsers.Count(), Is.EqualTo(0));
                Assert.That(item.CustomLookup, Is.Null);
                Assert.That(item.CustomMultiLookup.Count(), Is.EqualTo(0));
                Assert.That(item.CustomDate, Is.Null);
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        #endregion

        #region Delete Tests
        [Test]
        public void Delete_By_Entity_Deletes_Item_Test()
        {
            var item = new Item { Title = "Delete_By_Entity_Deletes_Item_Test" };
            _list.Add(item);
            Assert.That(item.Id, Is.Not.EqualTo(0));

            var items = _list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Delete_By_Entity_Deletes_Item_Test")))))
                .ToList();

            CollectionAssert.IsNotEmpty(items);
            Assert.That(items.First().Id, Is.EqualTo(item.Id));
            Assert.That(items.First().Title, Is.EqualTo("Delete_By_Entity_Deletes_Item_Test"));

            _list.Delete(items.First(), false);

            items = _list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Delete_By_Entity_Deletes_Item_Test")))))
                .ToList();

            CollectionAssert.IsEmpty(items);
        }
        [Test]
        public void Delete_By_Id_Deletes_Item_Test()
        {
            var item = new Item { Title = "Delete_By_Id_Deletes_Item_Test" };
            _list.Add(item);
            Assert.That(item.Id, Is.Not.EqualTo(0));

            var items = _list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Delete_By_Id_Deletes_Item_Test")))))
                .ToList();

            CollectionAssert.IsNotEmpty(items);
            Assert.That(items.First().Id, Is.EqualTo(item.Id));
            Assert.That(items.First().Title, Is.EqualTo("Delete_By_Id_Deletes_Item_Test"));

            _list.Delete(items.First().Id, false);

            items = _list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Delete_By_Id_Deletes_Item_Test")))))
                .ToList();

            CollectionAssert.IsEmpty(items);
        }
        #endregion

        #region Get Items Tests

        [Test]
        public void Cascade_Lookup_Test()
        {
            using (var tls = new TestListScope<CascadeLookup>("Cascade_Lookup_Test"))
            {
                var list = tls.List;

                var i1 = new CascadeLookup { Title = "1", };
                list.Add(i1);

                var i2 = new CascadeLookup { Title = "2", Parent = new CascadeLookup { Id = i1.Id } };
                list.Add(i2);

                var i3 = new CascadeLookup { Title = "3", Parent = new CascadeLookup { Id = i2.Id } };
                list.Add(i3);

                var ii3 = list.ById(i3.Id);
                var ii2 = ii3.Parent;
                var ii1 = ii3.Parent.Parent;

                Assert.That(i1.Guid, Is.EqualTo(ii1.Guid));
                Assert.That(i2.Guid, Is.EqualTo(ii2.Guid));
                Assert.That(i3.Guid, Is.EqualTo(ii3.Guid));
            }
        }


        [Test]
        public void Items_Returns_Items_By_CamlQuery_Test()
        {
            var item = new Item { Title = "Items_Returns_Items_By_CamlQuery_Test" };
            var item2 = new Item { Title = "Items_Returns_Items_By_CamlQuery_Test_2" };
            _list.Add(item);
            _list.Add(item2);
            Assert.That(item.Id, Is.Not.EqualTo(0));

            var items = _list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Items_Returns_Items_By_CamlQuery_Test_2")))))
                .ToList();

            CollectionAssert.IsNotEmpty(items);
            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That(items.First().Id, Is.EqualTo(item2.Id));
            Assert.That(items.First().Title, Is.EqualTo(item2.Title));
        }

        [Test]
        public void Items_Grouping_By_User_Field_Test()
        {
            IQueryList<Item> list = null;
            try
            {
                list = _queryWeb.Create<Item>("Items_Grouping_By_User_Field_Test");

                var item = new Item { Title = "Items_Returns_Items_By_CamlQuery_Test" };
                var item2 = new Item { Title = "Items_Returns_Items_By_CamlQuery_Test_2" };
                list.Add(item);
                list.Add(item2);
                Assert.That(item.Id, Is.Not.EqualTo(0));

                var items = list.Items(CamlQuery.Default).ToList();

                var grouped = items.GroupBy(i => i.Author,
                    (key, g) =>
                                new
                                {
                                    User = key,
                                    items = g.ToList()
                                }
                                ).ToArray();

                Assert.True(grouped.FirstOrDefault() != null);
                Assert.That(grouped.FirstOrDefault().items.Count, Is.EqualTo(2));
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }

        [Test]
        public void Items_Returns_Collection_Of_All_Content_Types_Test()
        {
            IQueryList<Item> list = null;
            try
            {
                list = _queryWeb.Create<Item>("Items_Returns_Collection_Of_All_Content_Types_Test");
                list.AddContentType<Announcement>();
                list.Add(new Item { Title = "Items_ReturnsCollectionOfAllContentTypesTest_Item" });
                list.Add(new Announcement
                {
                    Title = "Items_ReturnsCollectionOfAllContentTypesTest_Announcement",
                    Body = "Items_ReturnsCollectionOfAllContentTypesTest_Announcement",
                    Expires = DateTime.Now
                });

                var items = list.Items(CamlQuery.Default).ToList();

                Assert.IsNotNull(items);
                CollectionAssert.IsNotEmpty(items);

                Assert.That(items.FirstOrDefault(i => i.Title == "Items_ReturnsCollectionOfAllContentTypesTest_Item"), Is.Not.Null);
                Assert.That(items.FirstOrDefault(i => i.Title == "Items_ReturnsCollectionOfAllContentTypesTest_Announcement"), Is.Not.Null);
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }

        [Test]
        public void Items_Returns_Colection_Of_Custom_Items_Test()
        {
            IQueryList<CustomItem> list = null;
            try
            {
                list = _queryWeb.Create<CustomItem>("ListOfCustomItem");
                var customItem = new CustomItem
                    {
                        Title = "Items_ReturnsColectionOfCustomItemsTest",
                        CustomField1 = "Items_ReturnsColectionOfCustomItemsTest1",
                        CustomField2 = "Items_ReturnsColectionOfCustomItemsTest2",
                        CustomFieldNumber = 123.5,
                        CustomBoolean = true,
                        CustomChoice = TheChoice.Choice2,
                        Тыдыщ = "тест",
                        CustomUsers = new List<User>
                                          {
                                              new Person(_firstUser.LoginName),
                                              new Person(_domainGroup.LoginName),
                                              new User(_spGroup.Name),
                                          },
                    };
                list.Add(customItem);

                var customItem2 = new CustomItem
                {
                    Title = "Items_ReturnsColectionOfCustomItemsTest_2",
                    CustomField1 = "Items_ReturnsColectionOfCustomItemsTest1_2",
                    CustomField2 = "Items_ReturnsColectionOfCustomItemsTest2_2",
                    CustomFieldNumber = 155.5,
                    CustomBoolean = false,
                    CustomChoice = TheChoice.Choice3,
                    Тыдыщ = "тест2",
                };
                list.Add(customItem2);

                var items = list.Items(CamlQuery.Default).ToList();

                var item = items.FirstOrDefault();

                var lkp = item.CustomLookup;

                Assert.IsNotNull(item);
                Assert.That(customItem.Title, Is.EqualTo(item.Title));
                Assert.That(customItem.CustomField1, Is.EqualTo(item.CustomField1));
                Assert.That(customItem.CustomField2, Is.EqualTo(item.CustomField2));
                Assert.That(customItem.CustomFieldNumber, Is.EqualTo(item.CustomFieldNumber));
                Assert.That(customItem.CustomBoolean, Is.EqualTo(item.CustomBoolean));
                Assert.That(customItem.Тыдыщ, Is.EqualTo(item.Тыдыщ));
                Assert.That(item.CustomUser, Is.Null);
                Assert.That(item.CustomUsers, Is.Not.Null);

                var users = item.CustomUsers.ToList();

                Assert.That(item.CustomChoice, Is.EqualTo(item.CustomChoice));
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }

        [Test]
        public void Items_Returns_Collection_Of_Specific_ContentType_Test()
        {
            IQueryList<Item> list = null;
            try
            {
                list = _queryWeb.Create<Item>("Items_Returns_Collection_Of_Specific_ContentType_Test");
                list.AddContentType<Announcement>();

                list.Add(new Announcement
                        {
                            Title = "Items_ReturnsCollectionOfSpecificContentType",
                            Body = "Items_ReturnsCollectionOfSpecificContentType",
                            Expires = DateTime.Now,
                        });

                var items = list.Items<Announcement>(CamlQuery.Default).ToList();
                CollectionAssert.IsNotEmpty(items);
                Assert.That(items.Count(), Is.EqualTo(1));
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }

        [Test]
        public void Items_Returns_Collection_Of_Items_With_Field_Attribute_Test()
        {
            IQueryList<Item> list = null;
            try
            {
                list = _queryWeb.Create<Item>("Items_Returns_Collection_Of_Items_With_Field_Attribute_Test");
                list.AddContentType<Holiday>();
                var holiday = new Holiday
                    {
                        Title = "Items_ReturnsOfSpecificContentType",
                        Category = Category.Category2,
                        IsNonWorkingDay = true,
                        Date = DateTime.Now
                    };
                list.Add(holiday);
                Assert.That(holiday.Id, Is.Not.EqualTo(0));

                var item = list.Items<Holiday>(CamlQuery.Default).FirstOrDefault();
                Assert.IsNotNull(item);
                Assert.That(holiday.Category, Is.EqualTo(item.Category));
                Assert.That(holiday.IsNonWorkingDay, Is.EqualTo(item.IsNonWorkingDay));

                // sharepoint dont store ticks, that assert seconds
                var difference = holiday.Date.Subtract(item.Date);
                Assert.That(difference.Seconds, Is.EqualTo(0));
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }

        [Test]
        public void Items_Throws_On_No_Virtual_Property_Test()
        {
            IQueryList<CustomItemNoVirtualProperty> list = null;
            try
            {
                TestDelegate work = () =>
                {
                    list = _queryWeb.Create<CustomItemNoVirtualProperty>("Items_Throws_On_No_Virtual_Property_Test");

                    var customItem = new CustomItemNoVirtualProperty
                    {
                        Title = "Items_ReturnsColectionOfCustomItemsTest",
                        CustomField1 = "Items_ReturnsColectionOfCustomItemsTest1",
                        CustomField2 = "Items_ReturnsColectionOfCustomItemsTest2",
                        CustomFieldNumber = 123.5,
                        CustomBoolean = true,
                    };
                    list.Add(customItem);

                    var item = list.Items(CamlQuery.Default).FirstOrDefault();
                };
                var exc = Assert.Throws<SharepointCommonException>(work);
                Assert.That(exc.Message, Is.EqualTo("Property CustomField1 must be virtual to work correctly."));
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }
        
        [Test]
        public void ById_Returns_Entity_Test()
        {
            var item = new Item { Title = "ById_Returns_Entity_Test"};
            _list.Add(item);
            Assert.That(item.Id, Is.Not.EqualTo(0));

            var item2 = _list.ById(item.Id);

            Assert.That(item2, Is.Not.Null);
            Assert.That(item2.Id, Is.EqualTo(item.Id));
            Assert.That(item2.Title, Is.EqualTo("ById_Returns_Entity_Test"));
        }

        [Test]
        public void ById_Returns_Item_Of_ContentType_Test()
        {
            IQueryList<Item> list = null;
            try
            {
                list = _queryWeb.Create<Item>("ById_Returns_Item_Of_ContentType_Test");
                list.AddContentType<Announcement>();

                var announcement = new Announcement
                    {
                        Title = "ById_Returns_Item_Of_ContentType_Test",
                        Body = "ById_Returns_Item_Of_ContentType_Test",
                        Expires = DateTime.Now,
                    };

                list.Add(announcement);

                var item = list.ById<Announcement>(announcement.Id);

                Assert.That(item, Is.Not.Null);
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }

        [Test]
        public void ById_Throws_If_Item_Has_Different_ContentType_Test()
        {
            IQueryList<Item> list = null;
            try
            {
                list = _queryWeb.Create<Item>("ById_Returns_Item_Of_ContentType_Test");
                list.AddContentType<Announcement>();

                var item = new Item
                    {
                        Title = "ById_Returns_Item_Of_ContentType_Test_Item",
                    };
                list.Add(item);

                var announcement = new Announcement
                {
                    Title = "ById_Returns_Item_Of_ContentType_Test",
                    Body = "ById_Returns_Item_Of_ContentType_Test",
                    Expires = DateTime.Now,
                };
                list.Add(announcement);

                TestDelegate test = () => list.ById<Announcement>(item.Id);
                Assert.Throws<SharepointCommonException>(test);
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }

        [Test]
        public void ById_Returns_Null_On_Id_Not_Found_Test()
        {
            var item = _list.ById(int.MaxValue);
            Assert.That(item, Is.Null);
        }

        [Test]
        public void ByGuid_Returns_Entity_Test()
        {
            var item = new Item { Title = "ByGuid_Returns_Entity_Test" };
            _list.Add(item);
            Assert.That(item.Id, Is.Not.EqualTo(0));

            var item2 = _list.ByGuid(item.Guid);

            Assert.That(item2, Is.Not.Null);
            Assert.That(item2.Id, Is.EqualTo(item.Id));
            Assert.That(item2.Title, Is.EqualTo("ByGuid_Returns_Entity_Test"));
        }

        [Test]
        public void ByGuid_Throws_If_Item_Has_Different_ContentType_Test()
        {
            IQueryList<Item> list = null;
            try
            {
                list = _queryWeb.Create<Item>("ByGuid_Throws_If_Item_Has_Different_ContentType_Test");
                list.AddContentType<Announcement>();

                var item = new Item
                {
                    Title = "ByGuid_Throws_If_Item_Has_Different_ContentType_Test_Item",
                };
                list.Add(item);

                var announcement = new Announcement
                {
                    Title = "ByGuid_Throws_If_Item_Has_Different_ContentType_Test",
                    Body = "ByGuid_Throws_If_Item_Has_Different_ContentType_Test",
                    Expires = DateTime.Now,
                };
                list.Add(announcement);

                TestDelegate test = () => list.ByGuid<Announcement>(item.Guid);
                Assert.Throws<SharepointCommonException>(test);
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }

        [Test]
        public void ByGuid_Returns_Item_Of_ContentType_Test()
        {
            IQueryList<Item> list = null;
            try
            {
                list = _queryWeb.Create<Item>("ByGuid_Returns_Item_Of_ContentType_Test");
                list.AddContentType<Announcement>();

                var announcement = new Announcement
                {
                    Title = "ByGuid_Returns_Item_Of_ContentType_Test",
                    Body = "ByGuid_Returns_Item_Of_ContentType_Test",
                    Expires = DateTime.Now,
                };

                list.Add(announcement);

                var item = list.ByGuid<Announcement>(announcement.Guid);

                Assert.That(item, Is.Not.Null);
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }

        [Test]
        public void ByGuid_Returns_Null_On_Guid_not_Found_Test()
        {
            var item = _list.ByGuid(Guid.NewGuid());
            Assert.That(item, Is.Null);
        }

        [Test]
        public void ByField_Returns_Entity_Test()
        {
            var item1 = new Item { Title = "ByField_Returns_Entity_Test" };
            var item2 = new Item { Title = "ByField_Returns_Entity_Test" };
            _list.Add(item1);
            _list.Add(item2);

            Assert.That(item1.Id, Is.Not.EqualTo(0));
            Assert.That(item2.Id, Is.Not.EqualTo(0));

            var items = _list.ByField(i => i.Title, "ByField_Returns_Entity_Test");

            CollectionAssert.IsNotEmpty(items);
            Assert.That(items.Count(), Is.EqualTo(2));

            items = _list.ByField(i => i.Author, new User(@"sharepoint\system"));
            CollectionAssert.IsNotEmpty(items);

            items = _list.ByField(i => i.Author, new User(@"andproject\sharepoint"));
            CollectionAssert.IsEmpty(items);
            
        }

        #endregion

        #region Misc Tests
        [Test]
        public void FormUrlTest()
        {
            string newUrl = _list.FormUrl(PageType.New);
            string dispUrl = _list.FormUrl(PageType.Display, 2);
            string editUrl = _list.FormUrl(PageType.Edit, 3);

            var listsPath = System.IO.Path.Combine(_list.ParentWeb.Web.ServerRelativeUrl, "lists");

            Assert.That(newUrl.ToLower(),
                Is.EqualTo(string.Format("{1}/{0}/NewForm.aspx", ListName1, listsPath).ToLower()));

            Assert.That(dispUrl.ToLower(),
                Is.EqualTo(string.Format("{1}/{0}/DispForm.aspx?ID=2", ListName1, listsPath).ToLower()));

            Assert.That(editUrl.ToLower(),
                Is.EqualTo(string.Format("{1}/{0}/EditForm.aspx?ID=3", ListName1, listsPath).ToLower()));

            string newUrlIsDlg = _list.FormUrl(PageType.New, isDlg: true);
            string dispUrlIsDlg = _list.FormUrl(PageType.Display, 2, true);
            string editUrlIsDlg = _list.FormUrl(PageType.Edit, 3, true);

            Assert.That(newUrlIsDlg.ToLower(),
                Is.EqualTo(string.Format("{1}/{0}/NewForm.aspx?isDlg=1", ListName1, listsPath).ToLower()));

            Assert.That(dispUrlIsDlg.ToLower(),
                Is.EqualTo(string.Format("{1}/{0}/DispForm.aspx?ID=2&isDlg=1", ListName1, listsPath).ToLower()));

            Assert.That(editUrlIsDlg.ToLower(),
                Is.EqualTo(string.Format("{1}/{0}/EditForm.aspx?ID=3&isDlg=1", ListName1, listsPath).ToLower()));
        }

        [Test]
        public void UrlTest()
        {
            Assert.That(_list.Url.ToLower(),
                Is.EqualTo(string.Format("{0}/lists/{1}", _list.ParentWeb.Web.Url, ListName1).ToLower()));
        }

        [Test]
        public void RelativeUrlTest()
        {
            Assert.That(
                _list.RelativeUrl.ToLower(),
                Is.EqualTo(string.Format("lists/{0}", ListName1).ToLower()));
        }

        [Test]
        public void ListItem_Returns_SPListItem_Test()
        {
            var item = new Item { Title = "ListItem_Returns_SPListItem_Test" };
            _list.Add(item);
            Assert.That(item.Id, Is.Not.EqualTo(0));

            var items = _list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef<Item>(i => i.Title), Q.Value("ListItem_Returns_SPListItem_Test")))))
                .ToList();

            CollectionAssert.IsNotEmpty(items);
            var first = items.First();
            Assert.That(first.Id, Is.EqualTo(item.Id));
            Assert.That(first.Title, Is.EqualTo("ListItem_Returns_SPListItem_Test"));

            var spListItem = first.ListItem;
            Assert.NotNull(spListItem);
        }

        [Test]
        public void ParentWeb_Not_Null_Test()
        {
            var pw = _list.ParentWeb;
            Assert.NotNull(pw);
        }

        [Test]
        public void Throw_If_Access_Field_Not_In_ViewFields_Test()
        {
            IQueryList<CustomItem> list = null;
            try
            {
                list = _queryWeb.Create<CustomItem>("Throw_If_Access_Field_Not_In_ViewFields_Test");
                var customItem = new CustomItem
                {
                    Title = "Throw_If_Access_Field_Not_In_ViewFields_Test",
                    CustomField1 = "Throw_If_Access_Field_Not_In_ViewFields_Test1",
                    CustomFieldNumber = 123.5,
                    CustomBoolean = true,
                    CustomDate = DateTime.Now,
                    CustomChoice = TheChoice.Choice2,
                };
                list.Add(customItem);

                var item = list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef<CustomItem>(i => i.Title), Q.Value("Throw_If_Access_Field_Not_In_ViewFields_Test"))))
                .ViewFields<CustomItem>(i => i.CustomField1))
                .FirstOrDefault();

                Assert.IsNotNull(item);
                string ss;
                Assert.Throws<ArgumentException>(() => ss = item.CustomField2);
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Get_Deleted_Lookup_Value_Returns_Null_Test()
        {
            IQueryList<LookupWithShowField> list = null;
            try
            {
                var lookupItemBeenDeleted = new Item { Title = "aaa" };
                _listForLookup.Add(lookupItemBeenDeleted);

                list = _queryWeb.Create<LookupWithShowField>("Get_Deleted_Lookup_Value_Returns_Null_Test");

                var item = new LookupWithShowField { CustomLookup = lookupItemBeenDeleted, };
                list.Add(item);

                _listForLookup.Delete(lookupItemBeenDeleted, false);

                item = list.ById(item.Id);

                Assert.DoesNotThrow(() => { var cl = item.CustomLookup; });
                Assert.Null(item.CustomLookup);
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        #endregion

        #region Fields Tests
        [Test]
        public void EnsureField_Adds_Field_Test()
        {
            var list = _queryWeb.GetByName<CustomItem>(ListName1);
            list.EnsureField(e => e.CustomField1);
            list.EnsureField(e => e.CustomLookup);

            Assert.That(list.ContainsField(e => e.CustomField1));
            Assert.That(list.ContainsField(e => e.CustomLookup));
        }

        [Test]
        public void EnsureField_Sets_Default_Value_Test()
        {
            var list = _queryWeb.GetByName<CustomItem>(ListName1);
            list.EnsureField(e => e.WithDefault);

            Assert.That(list.ContainsField(e => e.WithDefault));

            var field = list.GetField(e => e.WithDefault);

            Assert.That(field.DefaultValue, Is.False);
        }

        [Test]
        public void EnsureFields_Adds_All_Fields_Test()
        {
            var list = _queryWeb.GetByName<CustomItem>(ListName1);
            list.EnsureFields();
            Assert.That(list.ContainsField(e => e.CustomField1));
            Assert.That(list.ContainsField(e => e.CustomField2));
            Assert.That(list.ContainsField(e => e.CustomBoolean));
            Assert.That(list.ContainsField(e => e.CustomFieldNumber));
            Assert.That(list.ContainsField(e => e.CustomUser));
            Assert.That(list.ContainsField(e => e.CustomUsers));
            Assert.That(list.ContainsField(e => e.CustomLookup));
            Assert.That(list.ContainsField(e => e.CustomChoice));
            list.EnsureFields();
        }

        [Test]
        public void CheckFields_Does_Not_Throws_If_All_Fields_Exists_Test()
        {
            var list = _queryWeb.GetByName<CustomItem>(ListName1);
            list.EnsureFields();
            list.CheckFields();
        }

        [Test]
        public void CheckFields_Throws_If_Field_Not_Exists_Test()
        {
            TestDelegate test = () =>
                {
                    IQueryList<CustomItem> list = null;
                    try
                    {
                        _queryWeb.Create<Item>("CheckFields_Does_Throws_If_Field_Not_Exists_Test");
                        list = _queryWeb.GetByName<CustomItem>("CheckFields_Does_Throws_If_Field_Not_Exists_Test");
                        list.CheckFields();
                    }
                    finally
                    {
                        if (list != null) list.DeleteList(false);
                    }
                };
            
            Assert.Throws<SharepointCommonException>(test);
        }

        [Test]
        public void DisplayName_Applies_Test()
        {
            IQueryList<CustomItem> list = null;

            try
            {
                list = _queryWeb.Create<CustomItem>("List785");

                var field = list.GetField(ci => ci.CustomField2);

                Assert.That(field.DisplayName, Is.EqualTo("Многостр.текст"));
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }

        [Test]
        public void Reqired_Applies_Test()
        {
            IQueryList<CustomItem> list = null;

            try
            {
                list = _queryWeb.Create<CustomItem>("List786");

                var field = list.GetField(ci => ci.CustomField1);

                Assert.That(field.Required, Is.True);
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }

        [Test]
        public void GetField_Returns_Field_Test()
        {
            var field = _list.GetField(i => i.Title);

            Assert.That(field, Is.Not.Null);
            Assert.That(field.Name, Is.EqualTo("Title"));
            Assert.That(field.Type, Is.EqualTo(Microsoft.SharePoint.SPFieldType.Text));
        }

        [Test]
        public void GetFields_Returns_Fields_Test()
        {
            IQueryList<CustomItem> list = null;
            try
            {
                list = _queryWeb.Create<CustomItem>("GetFields_Returns_Fields_Test");
                var fields = list.GetFields(true).ToList();

                Assert.That(fields, Is.Not.Null);
                CollectionAssert.IsNotEmpty(fields);
                Assert.That(fields.Any(f => f.Name.Equals("CustomField1")), Is.True);
                Assert.That(fields.Any(f => f.Name.Equals("HTML_x0020_File_x0020_Type")), Is.False);
                Assert.That(fields.All(field => field.Id != default(Guid)));

                var fieldWithAttr = list.GetField(i => i.Тыдыщ);
                Assert.NotNull(fieldWithAttr);
                Assert.That(fieldWithAttr.Name, Is.EqualTo("_x0422__x044b__x0434__x044b__x04"));
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Item_With_Not_Mapped_Props_Test()
        {
            IQueryList<ItemWithNoMappedProperty> list = null;
            try
            {
                list = _queryWeb.Create<ItemWithNoMappedProperty>("Item_With_Not_Mapped_Props_Test");

                Assert.IsFalse(list.ContainsField(i => i.NotField));
                Assert.IsFalse(list.ContainsField(i => i.NotMapped));


                var item = new ItemWithNoMappedProperty
                    {
                        NotField = "asd",
                        NotMapped = "zxc",
                    };
                list.Add(item);

                var item2 = list.ById(item.Id);

                Assert.IsNull(item2.NotField);
                Assert.IsNull(item2.NotMapped);
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        #endregion

        [Test]
        public void List_Save_Conflict_Successfully_Resolved_Test()
        {
            const string Listname = "Items_Returns_Collection_Of_All_Content_Types_Test";
            IQueryList<Item> list = null;
            try
            {
                TestDelegate test = () =>
                {
                    list = _queryWeb.Create<Item>(Listname);
                    var list1 = _queryWeb.GetByName<Item>(Listname);
                    var list2 = _queryWeb.GetByName<Item>(Listname);
                    var list3 = _queryWeb.GetByName<Item>(Listname);
                    var list4 = _queryWeb.GetByName<Item>(Listname);
                    
                    list.AllowManageContentTypes = true;
                    list1.IsVersioningEnabled = true;
                    list2.IsFolderCreationAllowed = true;
                    list3.Title = "OtherTitle";
                    list4.AddContentType<Announcement>();
                };
                Assert.DoesNotThrow(test);
            }
            finally
            {
                if (list != null) list.DeleteList(false);
            }
        }

        [Test]
        public void Item_ParentList_Contains_Reference_To_ParentList()
        {
            var itm = new Item { Title = "Item_ParentList_Contains_Reference_To_ParentList" };
            _list.Add(itm);

            Assert.That(itm.ParentList, Is.Not.Null);

            var itm2 = _list.ById(itm.Id);

            Assert.That(itm2.ParentList, Is.Not.Null);
            Assert.That(itm2.ParentList.Id, Is.EqualTo(_list.Id));
        }

        [Test]
        public void Item_ConcreteParentList_Contains_Reference_To_ParentList()
        {
            IQueryList<CustomItem> list = null;
            try
            {
                list = _queryWeb.Create<CustomItem>("Item_ConcreteParentList_Contains_Reference_To_ParentList");
                var itm = new CustomItem { Title = "Item_ConcreteParentList_Contains_Reference_To_ParentList" };
                list.Add(itm);

                Assert.That(itm.ConcreteParentList, Is.Not.Null);
                Assert.IsInstanceOf<ListBase<CustomItem>>(itm.ConcreteParentList);
                Assert.That(((IQueryList<CustomItem>)itm.ConcreteParentList).Id, Is.EqualTo(list.Id));

                var itm2 = list.ById(itm.Id);

                Assert.That(itm2.ConcreteParentList, Is.Not.Null);
                Assert.IsInstanceOf<ListBase<CustomItem>>(itm2.ConcreteParentList);
                Assert.That(((IQueryList<CustomItem>)itm2.ConcreteParentList).Id, Is.EqualTo(list.Id));
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Use_Person_As_Property_Throws_Test()
        {
            IQueryList<OneMoreField<Person>> list = null;
            try
            {
                TestDelegate test = () =>
                {
                    list = _queryWeb.Create<OneMoreField<Person>>("Use_Person_As_Property_Throws_Test");
               
                    var customItem = new OneMoreField<Person>
                    {
                        Title = "Items_ReturnsColectionOfCustomItemsTest",
                        AdditionalField = new Person("test@test.com")
                    };
                    list.Add(customItem);
                };
                Assert.Throws<SharepointCommonException>(test);
            }
            finally
            {
                if (list != null)
                {
                    list.DeleteList(false);
                }
            }
        }
    }
}