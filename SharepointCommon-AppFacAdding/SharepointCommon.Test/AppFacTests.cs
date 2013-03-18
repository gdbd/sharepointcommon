using System;
using NUnit.Framework;

namespace SharepointCommon.Test
{
    [TestFixture]
    public class AppFacTests
    {
        [Test]
        public void GetCurrentTest()
        {
            var app = TestAppFac.Open("asd");
        }
    }

    public class TestAppFac : AppFac<TestApp>
    {
    }

    public class TestApp : AppBase
    {
        public TestApp(IQueryWeb queryWeb) : base(queryWeb)
        {
        }
    }
}
