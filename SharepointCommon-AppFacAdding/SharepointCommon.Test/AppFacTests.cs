using System;
using NUnit.Framework;
using SharepointCommon.Attributes;

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

        private class TestApp : AppBase<TestApp>
        {
            [List(Name = "SiteUserInfoList")]
            public virtual IQueryList<Item> UserInfoList { get; set; }
        }
    }
}
