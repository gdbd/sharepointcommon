using System;
using System.Linq;
using Microsoft.SharePoint;
using NUnit.Framework;
using SharepointCommon.Test.Application;
using SharepointCommon.Test.Entity;
using SharepointCommon.Test.Repository;

namespace SharepointCommon.Test
{
    [TestFixture]
    public class AppFacTests
    {
        private readonly string _webUrl = Settings.GetTestSiteCollectionUrl();

        [Test]
        public void AppBase_Factory_OpenByUrl_Test()
        {
            using (var app01 = TestApp.Factory.OpenNew(_webUrl))
            {
                Assert.NotNull(app01);
            }
        }

        [Test]
        public void AppBase_Factory_OpenBySPWeb_Test()
        {
            using (var site = new SPSite(_webUrl))
            {
                using (var web = site.OpenWeb())
                {
                    var app = TestApp.Factory.ExistingWeb(web);
                    Assert.NotNull(app);
                }
            }

            using (var app01 = TestApp.Factory.OpenNew(_webUrl))
            {
                Assert.NotNull(app01);
            }
        }

        [Test]
        public void AppBase_Get_UserInfoList_Test()
        {
            using (var app01 = TestApp.Factory.OpenNew(_webUrl))
            {    
                var userUnfoList = app01.UserInfoList;
                Assert.NotNull(userUnfoList);

                var user = userUnfoList.Items(CamlQuery.Default).FirstOrDefault();

                Assert.NotNull(user);
            }
        }

        [Test]
        public void AppBase_Get_List_Twice_Returns_Cached_Test()
        {
            using (var app01 = TestApp.Factory.OpenNew(_webUrl))
            {
                var userUnfoList = app01.UserInfoList;
                var userUnfoList2 = app01.UserInfoList;
                
                Assert.AreSame(userUnfoList, userUnfoList2);
            }
        }

        [Test]
        public void AppBase_Set_List_Throws_Test()
        {
            using (var app01 = TestApp.Factory.OpenNew(_webUrl))
            {
                Assert.Throws<SharepointCommonException>(() => app01.UserInfoList = null);
                Assert.DoesNotThrow(() => app01.Test2 = null);
                Assert.DoesNotThrow(() => app01.Test3 = null);
            }
        }

        [Test]
        public void AppBase_Get_List_Throws_On_NoVirtual_Test()
        {
            Assert.Throws<SharepointCommonException>(() =>
                {
                    using (var app01 = TestAppNoVirtualProperty.Factory.OpenNew(_webUrl))
                    {
                    }
                });
        }

        [Test]
        public void AppBase_List_With_NotMapped_Test()
        {
            using (var app01 = TestAppNotMappedList.Factory.OpenNew(_webUrl))
            {
                var userlist = app01.UserInfoList;
                app01.Test = userlist;
            }
        }

        [Test]
        public void AppBase_ApplicationRespository_Test()
        {
            using (var app = AppWithRepository.Factory.OpenNew(_webUrl))
            {
                IQueryList<OneMoreField<string>> queryList = null;
                try
                {
                    queryList = app.QueryWeb.Create<OneMoreField<string>>("CustomItems");

                    var ci = app.CustomItems;

                    var item = new OneMoreField<string>
                    {
                        Title = "Test",
                        AdditionalField = "Test2",
                    };

                    ci.Add(item);
                }
                finally
                {
                    if (queryList != null) queryList.DeleteList(false);
                }
            }
        }

        [Test]
        public void Ensure_List_By_Name_Test()
        {
            using (var app = TestAppEnsureLists.Factory.OpenNew(_webUrl))
            {
                IQueryList<OneMoreField<string>> list = null;
                try
                {
                    list = app.EnsureList(a => a.EnsureByName);

                    Assert.NotNull(list);
                    Assert.That(list.Title, Is.EqualTo("List ensured by name"));
                    Assert.That(list.RelativeUrl.ToLower(), Is.EqualTo("lists/ensurebyname"));

                    IQueryList<OneMoreField<string>> list2 = null;
                    Assert.DoesNotThrow(() =>
                    {
                        list2 = app.EnsureList(a => a.EnsureByName);
                    });

                    Assert.That(list.Id == list2.Id);
                }
                finally
                {
                    if (list != null) list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Ensure_List_By_Url_Test()
        {
            using (var app = TestAppEnsureLists.Factory.OpenNew(_webUrl))
            {
                IQueryList<Item> list = null;
                try
                {
                    list = app.EnsureList(a => a.EnsureByUrl);

                    Assert.NotNull(list);
                    Assert.That(list.Title, Is.EqualTo("EnsureByUrl"));
                    Assert.That(list.RelativeUrl.ToLower(), Is.EqualTo("lists/ensurebyurl"));

                    IQueryList<Item> list2 = null;
                    Assert.DoesNotThrow(() =>
                    {
                        list2 = app.EnsureList(a => a.EnsureByUrl);
                    });

                    Assert.That(list.Id == list2.Id);
                }
                finally
                {
                    if (list != null) list.DeleteList(false);
                }
            }
        }

        [Test]
        public void Ensure_List_By_Id_Throws_Test()
        {
            using (var app = TestAppEnsureLists.Factory.OpenNew(_webUrl))
            {
                Assert.Throws<SharepointCommonException>(() => app.EnsureList(a => a.EnsureById));
            }
        }
    }
}
