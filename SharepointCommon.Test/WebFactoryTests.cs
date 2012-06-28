namespace SharepointCommon.Test
{
    using System;

    using NUnit.Framework;

    using SharepointCommon.Entities;
    using SharepointCommon.Test.Entity;

    [TestFixture]
    public class WebFactoryTests
    {
        private readonly string _webUrl = string.Format("http://{0}/", Environment.MachineName);

        [Test]
        public void Create_Creates_List_Test()
        {
            using (var factory = WebFactory.Open(_webUrl))
            {
                IQueryList<Item> list = null;
                try
                {
                    list = factory.Create<Item>("List754");
                    Assert.AreEqual(list.Title, "List754");

                    Assert.That(list.ContainsContentType<Item>());
                }
                finally
                {
                    if (null != list) list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Create_Creates_List_With_Custom_Fields_Test()
        {
            using (var factory = WebFactory.Open(_webUrl))
            {
                var list1 = factory.Create<Item>("ListForLookup");
                var list = factory.Create<CustomItem>("List755");

                Assert.AreEqual(list.Title, "List755");

                Assert.That(list1.ContainsContentType<Item>());

                Assert.That(list.ContainsField(ci => ci.CustomField1), Is.True);
                Assert.That(list.ContainsField(ci => ci.CustomField2), Is.True);
                Assert.That(list.ContainsField(ci => ci.CustomFieldNumber), Is.True);
                Assert.That(list.ContainsField(ci => ci.CustomBoolean), Is.True);
                Assert.That(list.ContainsField(ci => ci.CustomUser), Is.True);
                Assert.That(list.ContainsField(ci => ci.CustomUsers), Is.True);
                Assert.That(list.ContainsField(ci => ci.CustomLookup), Is.True);
                Assert.That(list.ContainsField(ci => ci.CustomMultiLookup), Is.True);

                list.DeleteList(false);
                list1.DeleteList(false);
            }
        }
        
        [Test]
        public void Create_Creates_List_With_ContentType_Test()
        {
            using (var factory = WebFactory.Open(_webUrl))
            {
                IQueryList<Announcement> list = null;
                try
                {
                    list = factory.Create<Announcement>("List756");
                    Assert.AreEqual(list.Title, "List756");
                    Assert.That(list.ContainsContentType<Item>(), Is.False);
                    Assert.That(list.ContainsContentType<Announcement>());
                    Assert.That(list.ContainsField(a => a.Body));
                }
                finally
                {
                    if (null != list) list.DeleteList(false);
                }
            }
        }


        [Test]
        public void Create_Creates_DocLibrary_Test()
        {
            using (var factory = WebFactory.Open(_webUrl))
            {
                var list = factory.Create<Document>("List757");
                Assert.AreEqual(list.Title, "List757");
                Assert.That(list.ContainsContentType<Document>());
                list.DeleteList(false);
            }
        }

        [Test]
        public void Create_Creates_DocLibrary_With_ContentType_Test()
        {
            using (var factory = WebFactory.Open(_webUrl))
            {
                IQueryList<Form> list = null;
                try
                {
                    list = factory.Create<Form>("List758");
                    Assert.AreEqual(list.Title, "List758");
                    Assert.That(list.ContainsContentType<Item>(), Is.False);
                    Assert.That(list.ContainsContentType<Form>());
                }
                finally
                {
                    if (null != list) list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Create_Creates_DocLibrary_With_Custom_Fields_Test()
        {
            using (var factory = WebFactory.Open(_webUrl))
            {
                IQueryList<Item> list1 = null;
                IQueryList<CustomDocument> list = null;

                try
                {
                    list1 = factory.Create<Item>("ListForLookup");
                    list = factory.Create<CustomDocument>("List759");

                    Assert.AreEqual(list.Title, "List759");

                    Assert.That(list.ContainsContentType<Item>() == false);
                    Assert.That(list.ContainsContentType<Document>());

                    Assert.That(list.ContainsField(ci => ci.CustomField1), Is.True);
                    Assert.That(list.ContainsField(ci => ci.CustomField2), Is.True);
                    Assert.That(list.ContainsField(ci => ci.CustomFieldNumber), Is.True);
                    Assert.That(list.ContainsField(ci => ci.CustomBoolean), Is.True);
                    Assert.That(list.ContainsField(ci => ci.CustomUser), Is.True);
                    Assert.That(list.ContainsField(ci => ci.CustomUsers), Is.True);
                    Assert.That(list.ContainsField(ci => ci.CustomLookup), Is.True);
                    Assert.That(list.ContainsField(ci => ci.CustomMultiLookup), Is.True);
                }
                finally
                {
                    if (list != null)
                    {
                        list.DeleteList(false);
                    }
                    if (list1 != null)
                    {
                        list1.DeleteList(false);
                    }
                }
            }
        }
        
        [Test]
        public void ExistsByUrl_Returns_Value_Test()
        {
            using (var factory = WebFactory.Open(_webUrl))
            {
                IQueryList<Item> list = null;

                try
                {
                    list = factory.Create<Item>("ExistsByUrl_Returns_True_When_List_Exists_Test");
                    bool exists = factory.ExistsByUrl(list.RelativeUrl);
                    Assert.That(exists);

                    exists = factory.ExistsByUrl("lists/ExistsByUrl_Returns_True_When_List_Exists_Test_Not_Existing");
                    Assert.That(exists == false);
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
        public void ExistsByName_Returns_Value_Test()
        {
            using (var factory = WebFactory.Open(_webUrl))
            {
                IQueryList<Item> list = null;

                try
                {
                    list = factory.Create<Item>("ExistsByName_Returns_True_When_List_Exists_Test");
                    bool exists = factory.ExistsByName(list.Title);
                    Assert.That(exists);

                    exists = factory.ExistsByName("ExistsByName_Returns_True_When_List_Exists_Test_Not_Existing");
                    Assert.That(exists == false);
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
        public void ExistsById_Returns_Value_Test()
        {
            using (var factory = WebFactory.Open(_webUrl))
            {
                IQueryList<Item> list = null;

                try
                {
                    list = factory.Create<Item>("ExistsById_Returns_True_When_List_Exists_Test");
                    bool exists = factory.ExistsById(list.Id);
                    Assert.That(exists);

                    exists = factory.ExistsById(Guid.NewGuid());
                    Assert.That(exists == false);
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
}