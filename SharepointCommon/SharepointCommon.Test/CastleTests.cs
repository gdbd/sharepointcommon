namespace SharepointCommon.Test
{
    using Castle.DynamicProxy;
    using Moq;
    using NUnit.Framework;

    using SharepointCommon.Common;
    using SharepointCommon.Test.Entity;

    [TestFixture]
    public class CastleTests
    {
        [Test]
        public void Castle_Creates_Object_Proxy_Test()
        {
            var interceptorMock = new Mock<IInterceptor>();
            bool called = false;
            interceptorMock.Setup(i => i.Intercept(It.IsAny<IInvocation>()))
                .Callback(() => called = true);
                

            var generator = new ProxyGenerator();
            var customItem = generator.CreateClassProxy<CustomItem>(interceptorMock.Object);

            var s = customItem.CustomLookup;

            NUnit.Framework.Assert.That(called);
        }
    }
}