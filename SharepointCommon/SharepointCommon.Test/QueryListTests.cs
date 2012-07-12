namespace SharepointCommon.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.SharePoint;
    using NUnit.Framework;

    using SharepointCommon.Common;
    using SharepointCommon.Entities;
    using SharepointCommon.Exceptions;
    using SharepointCommon.Impl;
    using SharepointCommon.Test.Entity;

    using Assert = NUnit.Framework.Assert;

    [TestFixture]
    public class QueryListTests
    {
        private SPUser CurrentUserLogin;
        private SPUser SecondUserLogin;
        private const string ListName1 = "SharepointCommonTestList";
        private const string ListForLookup = "ListForLookup";
        private readonly string _webUrl = string.Format("http://{0}/", Environment.MachineName);
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
            
            var users = _queryWeb.Web.SiteUsers.Cast<SPUser>().Where(u => u.IsDomainGroup == false).ToList();

            CurrentUserLogin = users[0];
            SecondUserLogin = users[1];
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
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Add_AddsItemTest")))))
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
                        CustomUser = new User { Login = CurrentUserLogin.LoginName },
                        CustomUsers = new List<User> { new User { Login = CurrentUserLogin.LoginName } },
                        CustomLookup = lookupItem,
                        CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 },
                        CustomDate = DateTime.Now,
                        CustomChoice = TheChoice.Choice2,
                    };
                list.Add(customItem);

                var item = list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Items_ReturnsColectionOfCustomItemsTest")))))
                .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(customItem.Id));
                Assert.That(item.Title, Is.EqualTo(customItem.Title));
                Assert.That(item.CustomField1, Is.EqualTo(customItem.CustomField1));
                Assert.That(item.CustomField2, Is.EqualTo(customItem.CustomField2));
                Assert.That(item.CustomFieldNumber, Is.EqualTo(customItem.CustomFieldNumber));
                Assert.That(item.CustomBoolean, Is.EqualTo(customItem.CustomBoolean));
                Assert.That(item.CustomUser.Id, Is.Not.EqualTo(0));
                Assert.That(item.CustomUsers.Count(), Is.EqualTo(1));
                Assert.That(item.CustomUsers.First().Id, Is.EqualTo(CurrentUserLogin.ID));
                Assert.That(item.CustomLookup, Is.Not.Null);
                Assert.That(item.CustomLookup.Id, Is.EqualTo(lookupItem.Id));
                Assert.That(item.CustomMultiLookup, Is.Not.Null);
                Assert.That(item.CustomMultiLookup.Count(), Is.EqualTo(2));
                Assert.That(item.CustomMultiLookup.First().Title, Is.EqualTo(lookupItem.Title));
                Assert.That(item.CustomChoice, Is.EqualTo(customItem.CustomChoice));
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
                                CustomUser = new User { Login = CurrentUserLogin.LoginName },
                                CustomUsers = new List<User> { new User { Login = CurrentUserLogin.LoginName } },
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
                        CustomUser = new User { Login = CurrentUserLogin.LoginName },
                        CustomUsers = new List<User> { new User { Login = CurrentUserLogin.LoginName }, },
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
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Add_AddsItemOfContentTypeTest")))))
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
        #endregion

        #region Add Document Tests
        [Test]
        public void Add_Uploads_Document_Test()
        {
            IQueryList<Document> lib = null;
            try
            {
                lib = _queryWeb.Create<Document>("Add_AddsCustomItem");
                var document = new Document
                {
                    Name = "Add_AddsCustomItem.dat",
                    Content = new byte[] { 5, 10, 15, 25 },
                };
                lib.Add(document);

                var item = lib.Items(new CamlQuery()
                    .Query(Q.Where(Q.Eq(Q.FieldRef("LinkFilename"), Q.Value("Add_AddsCustomItem.dat")))))
                    .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(document.Id));
                Assert.That(item.Name, Is.EqualTo(document.Name));
                Assert.That(item.Content, Is.Not.Null);
                Assert.That(item.Content.Length, Is.EqualTo(document.Content.Length));
                Assert.That(item.Size, Is.EqualTo(4));
                Assert.That(item.Icon, Is.EqualTo("/_layouts/images/icgen.gif"));
                Assert.That(item.Folder, Is.EqualTo(document.Folder));
                //// Assert.That(item.Title, Is.EqualTo(document.Title));
            }
            finally
            {
                if (lib != null)
                {
                    lib.DeleteList(false);
                }
            }
        }

        [Test]
        public void Add_Uploads_Document_To_Folder_Test()
        {
            IQueryList<Document> lib = null;
            try
            {
                lib = _queryWeb.Create<Document>("Add_Uploads_Document_To_Folder_Test");
                var document = new Document
                {
                    Name = "Add_Uploads_Document_To_Folder_Test.dat",
                    Content = new byte[] { 5, 10, 15, 25 },
                    Folder = "Folder1/Folder2/Folder3",
                };
                lib.Add(document);

                var item = lib.Items(new CamlQuery()
                    .Recursive()
                  //  .Folder(document.Url)
                    .Query(Q.Where(Q.Eq(Q.FieldRef("LinkFilename"), Q.Value("Add_Uploads_Document_To_Folder_Test.dat")))))
                    .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(document.Id));
                Assert.That(item.Name, Is.EqualTo(document.Name));
                Assert.That(item.Content, Is.Not.Null);
                Assert.That(item.Content.Length, Is.EqualTo(document.Content.Length));
                Assert.That(item.Size, Is.EqualTo(4));
                Assert.That(item.Icon, Is.EqualTo("/_layouts/images/icgen.gif"));
                Assert.That(item.Folder, Is.EqualTo(document.Folder));
                //// Assert.That(item.Title, Is.EqualTo(document.Title));
            }
            finally
            {
                if (lib != null)
                {
                    lib.DeleteList(false);
                }
            }
        }

        [Test]
        public void Add_Uploads_CustomDocument_Test()
        {
            IQueryList<CustomDocument> list = null;
            try
            {
                var lookupItem = new Item { Title = "Add_Adds_CustomItem_Test_Lookup" };
                _listForLookup.Add(lookupItem);

                var lookupItem2 = new Item { Title = "Add_Adds_CustomItem_Test_Lookup_2" };
                _listForLookup.Add(lookupItem2);

                list = _queryWeb.Create<CustomDocument>("Add_Uploads_CustomDocument_Test");
                var customDoc = new CustomDocument
                {
                    Title = "Add_Uploads_CustomDocument_Test",
                    Name = "Add_Uploads_CustomDocument_Test",
                    Content = new byte[] { 5, 10, 15, 25 },
                    CustomField1 = "Add_Uploads_CustomDocument_Test1",
                    CustomField2 = "Add_Uploads_CustomDocument_Test2",
                    CustomFieldNumber = 123.5,
                    CustomBoolean = true,
                    CustomUser = new User { Login = CurrentUserLogin.LoginName },
                    CustomUsers = new List<User> { new User { Login = CurrentUserLogin.LoginName } },
                    CustomLookup = lookupItem,
                    CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 },
                    CustomDate = DateTime.Now,
                };
                list.Add(customDoc);

                var item = list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef("LinkFilename"), Q.Value("Add_Uploads_CustomDocument_Test")))))
                .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(customDoc.Id));

                Assert.That(item.Name, Is.EqualTo(customDoc.Name));
                Assert.That(item.Content, Is.Not.Null);
                Assert.That(item.Content.Length, Is.EqualTo(customDoc.Content.Length));
                Assert.That(item.Size, Is.EqualTo(4));
                Assert.That(item.Icon, Is.EqualTo("/_layouts/images/icgen.gif"));
                Assert.That(item.Folder, Is.EqualTo(customDoc.Folder));

                Assert.That(item.Title, Is.EqualTo(customDoc.Title));
                Assert.That(item.CustomField1, Is.EqualTo(customDoc.CustomField1));
                Assert.That(item.CustomField2, Is.EqualTo(customDoc.CustomField2));
                Assert.That(item.CustomFieldNumber, Is.EqualTo(customDoc.CustomFieldNumber));
                Assert.That(item.CustomBoolean, Is.EqualTo(customDoc.CustomBoolean));
                Assert.That(item.CustomUser.Id, Is.Not.EqualTo(0));
                Assert.That(item.CustomUsers.Count(), Is.EqualTo(1));
                Assert.That(item.CustomUsers.First().Id, Is.EqualTo(CurrentUserLogin.ID));
                Assert.That(item.CustomLookup, Is.Not.Null);
                Assert.That(item.CustomLookup.Id, Is.EqualTo(lookupItem.Id));
                Assert.That(item.CustomMultiLookup, Is.Not.Null);
                Assert.That(item.CustomMultiLookup.Count(), Is.EqualTo(2));
                Assert.That(item.CustomMultiLookup.First().Title, Is.EqualTo(lookupItem.Title));
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

        #region Update Tests
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
                    CustomUser = new User { Login = CurrentUserLogin.LoginName },
                    CustomUsers = new List<User> { new User { Login = CurrentUserLogin.LoginName }, },
                    CustomLookup = lookupItem,
                    CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 },
                    CustomDate = DateTime.Now,
                };
                list.Add(customItem);

                customItem.Title = "Update_Updates_CustomItem_Test_Updated";
                customItem.CustomField1 = "Update_Updates_CustomItem_Test1";
                customItem.CustomField2 = "Update_Updates_CustomItem_Test2";
                customItem.CustomFieldNumber = 235;
                customItem.CustomBoolean = false;
                customItem.CustomUser = new User { Login = SecondUserLogin.LoginName };
                customItem.CustomUsers = new List<User>
                    {
                        new User { Login = CurrentUserLogin.LoginName, },
                        new User { Login = SecondUserLogin.LoginName, }
                    };
                customItem.CustomLookup = lookupItem2;
                customItem.CustomMultiLookup = new List<Item> { lookupItem2 };

                list.Update(customItem, true);

                var item = list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"), Q.Value("Update_Updates_CustomItem_Test_Updated")))))
                .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(customItem.Id));
                Assert.That(item.Title, Is.EqualTo(customItem.Title));
                Assert.That(item.CustomField1, Is.EqualTo(customItem.CustomField1));
                Assert.That(item.CustomField2, Is.EqualTo(customItem.CustomField2));
                Assert.That(item.CustomFieldNumber, Is.EqualTo(customItem.CustomFieldNumber));
                Assert.That(item.CustomBoolean, Is.EqualTo(customItem.CustomBoolean));
                Assert.That(item.CustomUser, Is.Not.Null);
                Assert.That(item.CustomUser.Id, Is.Not.EqualTo(0));
                Assert.That(item.CustomUsers.Count(), Is.EqualTo(2));
                Assert.That(item.CustomUsers.First().Id, Is.EqualTo(CurrentUserLogin.ID));
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
                    CustomUser = new User { Login = CurrentUserLogin.LoginName },
                    CustomUsers = new List<User> { new User { Login = CurrentUserLogin.LoginName }, },
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
                Assert.That(item.CustomUser, Is.Null);
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
            var item = new Item { Title = "ById_Returns_Entity_Test" };
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
        }

        #endregion

        #region Misc methods
        [Test]
        public void FormUrlTest()
        {
            string newUrl = _list.FormUrl(PageType.New, 1);
            string dispUrl = _list.FormUrl(PageType.Display, 2);
            string editUrl = _list.FormUrl(PageType.Edit, 3);

            Assert.That(
                newUrl.ToLower(),
                Is.EqualTo(string.Format("/Lists/{0}/NewForm.aspx?ID=1&IsDlg=1", ListName1).ToLower()));

            Assert.That(
                dispUrl.ToLower(),
                Is.EqualTo(string.Format("/Lists/{0}/DispForm.aspx?ID=2&IsDlg=1", ListName1).ToLower()));

            Assert.That(
                editUrl.ToLower(),
                Is.EqualTo(string.Format("/Lists/{0}/EditForm.aspx?ID=3&IsDlg=1", ListName1).ToLower()));
        }

        [Test]
        public void UrlTest()
        {
            Assert.That(
                _list.Url.ToLower(),
                Is.EqualTo(string.Format("{0}lists/{1}", _webUrl, ListName1).ToLower()));
        }

        [Test]
        public void RelativeUrlTest()
        {
            Assert.That(
                _list.RelativeUrl.ToLower(),
                Is.EqualTo(string.Format("lists/{0}", ListName1).ToLower()));
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
        public void Item_ParentList_Contains_Reference_To_ParentList()
        {
            var itm = new Item { Title = "Item_ParentList_Contains_Reference_To_ParentList" };
            _list.Add(itm);

            var itm2 = _list.ById(itm.Id);

            Assert.That(itm2.ParentList, Is.Not.Null);
            Assert.That(itm2.ParentList.Id, Is.EqualTo(_list.Id));
        }

        [Test, Timeout(20000)]
        public void Get_Many_Items_Test()
        {
           // var splist = _queryWeb.Web.Lists["TestList1"];
           // var spitems = splist.Items.Cast<SPListItem>().ToList();

            // list with 50K items
            var list = _queryWeb.GetByName<TestList1>("TestList1");
            list.CheckFields();

            var items = list.Items(CamlQuery.Default);

            items = items.ToList();

            Assert.NotNull(items);
            CollectionAssert.IsNotEmpty(items);
            var first = items.First();

            Assert.That(first.Id, Is.Not.EqualTo(default(int)));
            Assert.That(first.Title, Is.Not.Null);
            Assert.That(first.TheText, Is.Not.Null);
            Assert.That(first.TheDate, Is.Not.EqualTo(default(DateTime)));
            Assert.That(first.TheMan, Is.Not.Null);
            Assert.That(first.TheLookup, Is.Not.Null);
        }
    }
}
