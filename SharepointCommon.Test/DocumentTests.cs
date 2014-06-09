using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint.Utilities;
using NUnit.Framework;
using SharepointCommon.Entities;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test
{
    [TestFixture]
    public class DocumentTests
    {
        private const string ListForLookup = "ListForLookup";
        private readonly string _webUrl = Settings.TestSiteUrl;
        private IQueryList<Item> _listForLookup;
        private IQueryWeb _queryWeb;

        [TestFixtureSetUp]
        public void Start()
        {
            _queryWeb = WebFactory.Open(_webUrl);
            
            if (_queryWeb.ExistsByName(ListForLookup))
            {
                _listForLookup = _queryWeb.GetByName<Item>(ListForLookup);
            }
            else
            {
                _listForLookup = _queryWeb.Create<Item>(ListForLookup);
            }
        }

        [TestFixtureTearDown]
        public void Stop()
        {
            _listForLookup.DeleteList(false);
            _queryWeb.Dispose();
        }
        
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
                    .Query(Q.Where(Q.Eq(Q.FieldRef<Document>(d => d.Name), Q.Value("Add_AddsCustomItem.dat")))))
                    .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(document.Id));
                Assert.That(item.Name, Is.EqualTo(document.Name));
                Assert.That(item.Content, Is.Not.Null);
                Assert.That(item.Content.Length, Is.EqualTo(document.Content.Length));
                Assert.That(item.Size, Is.EqualTo(4));
                Assert.That(item.Icon, Is.EqualTo("/_layouts/images/icgen.gif"));
                Assert.That(item.Folder, Is.EqualTo(document.Folder));
                Assert.NotNull(item.Url);
                Assert.That(item.Url, Is.EqualTo(SPUtility.ConcatUrls( _queryWeb.Web.ServerRelativeUrl , "/Add_AddsCustomItem/Add_AddsCustomItem.dat")));
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
                    .Query(Q.Where(Q.Eq(Q.FieldRef<Document>(d => d.Name), Q.Value("Add_Uploads_Document_To_Folder_Test.dat")))))
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
                    CustomLookup = lookupItem,
                    CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 },
                    CustomDate = DateTime.Now,
                };
                list.Add(customDoc);

                var item = list.Items(new CamlQuery()
                .Query(Q.Where(Q.Eq(Q.FieldRef<Document>(d => d.Name), Q.Value("Add_Uploads_CustomDocument_Test")))))
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

        [Test]
        public void Upload_Overwrite_WithoutVersioning_Test()
        {
            IQueryList<Document> lib = null;
            try
            {
                lib = _queryWeb.Create<Document>("Add_Uploads_Overwrite_Rename_Test");
                var document = new Document
                {
                    Name = "Doc1.dat",
                    Content = new byte[] { 5, 10, 15, 25 },
                };
                lib.Add(document);

                document = new Document
                {
                    Name = "Doc1.dat",
                    Content = new byte[] { 5, 10, 15, 27 },
                };

                lib.Add(document);

                var item = lib.Items(new CamlQuery()
                    .Query(Q.Where(Q.Eq(Q.FieldRef<Document>(d => d.Name), Q.Value("Doc1.dat")))))
                    .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(document.Id));
                Assert.That(item.Name, Is.EqualTo(document.Name));
                Assert.That(item.Version.Major, Is.EqualTo(1));
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
        public void Upload_Rename_WithoutVersioning_Test()
        {
            IQueryList<Document> lib = null;
            try
            {
                lib = _queryWeb.Create<Document>("Upload_Rename_WithoutVersioning_Test");
                var document = new Document
                {
                    Name = "Doc1.dat",
                    Content = new byte[] { 5, 10, 15, 25 },
                };
                lib.Add(document);

                var document2 = new Document
                {
                    Name = "Doc1.dat",
                    Content = new byte[] { 5, 10, 15, 27 },
                    RenameIfExists = true,
                };
                lib.Add(document2);

                var item = lib.Items(new CamlQuery()
                    .Query(Q.Where(Q.Eq(Q.FieldRef<Document>(d => d.Name), Q.Value("Doc1.dat")))))
                    .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(document.Id));
                Assert.That(item.Name, Is.EqualTo(document.Name));
                Assert.That(item.Version.Major, Is.EqualTo(1));

                var item2 = lib.Items(new CamlQuery()
                    .Query(Q.Where(Q.Eq(Q.FieldRef<Document>(d => d.Name), Q.Value("Doc1(1).dat")))))
                    .FirstOrDefault();

                Assert.IsNotNull(item2);
                Assert.That(item2.Id, Is.EqualTo(document2.Id));
                Assert.That(item2.Name, Is.EqualTo(document2.Name));
                Assert.That(item2.Version.Major, Is.EqualTo(1));
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
        public void Upload_Uploads_New_Version_Test()
        {
            IQueryList<Document> lib = null;
            try
            {
                lib = _queryWeb.Create<Document>("Upload_Uploads_New_Version_Test");
                lib.IsVersioningEnabled = true;
                var document = new Document
                {
                    Name = "Doc1.dat",
                    Content = new byte[] { 5, 10, 15, 25 },
                };
                lib.Add(document);

                document = new Document
                {
                    Name = "Doc1.dat",
                    Content = new byte[] { 5, 10, 15, 27 },
                };

                lib.Add(document);

                var item = lib.Items(new CamlQuery()
                    .Query(Q.Where(Q.Eq(Q.FieldRef<Document>(d => d.Name), Q.Value("Doc1.dat")))))
                    .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(document.Id));
                Assert.That(item.Name, Is.EqualTo(document.Name));
                Assert.That(item.Version.Major, Is.EqualTo(2));
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
        public void Upload_Rename_WithVersioning_Test()
        {
            IQueryList<Document> lib = null;
            try
            {
                lib = _queryWeb.Create<Document>("Upload_Rename_WithVersioning_Test");
                lib.IsVersioningEnabled = true;
                var document = new Document
                {
                    Name = "Doc1.dat",
                    Content = new byte[] { 5, 10, 15, 25 },
                };
                lib.Add(document);

                document = new Document
                {
                    Name = "Doc1.dat",
                    Content = new byte[] { 5, 10, 15, 27 },
                    RenameIfExists = true,
                };

                lib.Add(document);

                var item = lib.Items(new CamlQuery()
                    .Query(Q.Where(Q.Eq(Q.FieldRef<Document>(d => d.Name), Q.Value("Doc1(1).dat")))))
                    .FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.That(item.Id, Is.EqualTo(document.Id));
                Assert.That(item.Name, Is.EqualTo(document.Name));
                Assert.That(item.Version.Major, Is.EqualTo(1));
            }
            finally
            {
                if (lib != null)
                {
                    lib.DeleteList(false);
                }
            }
        }
    }
}
