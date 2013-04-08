using System;
using System.Linq;
using Microsoft.SharePoint;
using NUnit.Framework;

namespace SharepointCommon.Test
{
    [TestFixture]
    public class AppFacTests
    {
        private readonly string _webUrl = string.Format("http://{0}/", Environment.MachineName);

        [Test]
        public void AppBase_Factory_OpenByUrl_Test()
        {
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
                Assert.Throws<SharepointCommonException>(app01.SetListThatMustThrows);
                Assert.DoesNotThrow(app01.SetListThatMustNoThrows);
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
    }
}
