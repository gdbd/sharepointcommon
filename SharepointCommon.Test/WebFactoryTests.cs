namespace SharepointCommon.Test
{
    using System;
    using System.Linq;
    using System.Security.Principal;
    using Microsoft.SharePoint;

    using NUnit.Framework;

    using SharepointCommon.Entities;
    using SharepointCommon.Test.Entity;

    [TestFixture]
    public class WebFactoryTests
    {
        private readonly string _webUrl = string.Format("http://{0}/", Environment.MachineName);

        [Test]
        public void Open_By_Url_Creates_QueryWeb_Test()
        {
            using (var wf = WebFactory.Open(_webUrl))
            {
            }
        }

        [Test]
        public void Open_By_Ids_Creates_QueryWeb_Test()
        {
            Guid siteId, webId;
            using (var wf = WebFactory.Open(_webUrl))
            {
                siteId = wf.Site.ID;
                webId = wf.Web.ID;
            }

            using (var wf = WebFactory.Open(siteId, webId))
            {
            }
        }

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
                IQueryList<Item> list1;
                try
                {
                    list1 = factory.Create<Item>("ListForLookup");
                }
                catch (SPException)
                {
                    list1 = factory.GetByName<Item>("ListForLookup");
                }
                IQueryList<CustomItem> list = null;
                try
                {
                    list = factory.Create<CustomItem>("List755");

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
                    Assert.That(list.ContainsField(ci => ci.CustomChoice), Is.True);
                    Assert.That(list.ContainsField(ci => ci.Тыдыщ), Is.True);

                    var choiceField = list.GetField(ci => ci.CustomChoice);
                    Assert.NotNull(choiceField.Choices);
                    Assert.That(choiceField.Choices.Count(), Is.EqualTo(3));
                    var choiceWithName = choiceField.Choices.Skip(1).First();
                    Assert.That(choiceWithName, Is.EqualTo("The Choice Number Two"));
                }
                finally
                {
                    if (list != null)
                    {
                        list.DeleteList(false);
                    }
                    list1.DeleteList(false);
                }
            }
        }

        [Test]
        public void Create_Creates_List_With_Custom_Nullable_Test()
        {
            using (var factory = WebFactory.Open(_webUrl))
            {
                IQueryList<NullableItem> list = null;
                try
                {
                    list = factory.Create<NullableItem>("Create_Creates_List_With_Custom_Nullable_Test");

                    list.ContainsField(i => i.CustomDouble);
                    list.ContainsField(i => i.CustomInt);
                    list.ContainsField(i => i.CustomDecimal);
                    list.ContainsField(i => i.CustomBoolean);
                    list.ContainsField(i => i.CustomDate);
                    list.ContainsField(i => i.CustomChoice);
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
                    try
                    {
                        list1 = factory.Create<Item>("ListForLookup");
                    }
                    catch (SPException)
                    {
                        list1 = factory.GetByName<Item>("ListForLookup");
                    }
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
