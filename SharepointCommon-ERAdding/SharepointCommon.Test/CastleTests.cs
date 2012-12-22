namespace SharepointCommon.Test
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    using Castle.DynamicProxy;
    using Moq;
    using NUnit.Framework;

    using SharepointCommon.Attributes;
    using SharepointCommon.Common;
    using SharepointCommon.Entities;
    using SharepointCommon.Test.Entity;

    using Assert = NUnit.Framework.Assert;

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

            Assert.That(called);
        }

        [Test]
        public void Castle_Set_Value_To_Proxy_Test()
        {
            var interceptorMock = new Mock<IInterceptor>();
            bool called = false;
            interceptorMock.Setup(i => i.Intercept(It.IsAny<IInvocation>()))
                .Callback((IInvocation inv) =>
                    {
                        called = true;
                        inv.Proceed();
                    });
            
            var generator = new ProxyGenerator();
            var customItem = generator.CreateClassProxy<CustomItem>(interceptorMock.Object);

            customItem.Title = "test";

            Assert.That(called);
            Assert.That(customItem.Title, Is.EqualTo("test"));
        }

        [Test]
        public void Castle_Writes_Attribute_To_Proxy()
        {
            var generator = new ProxyGenerator();

            var interceptorMock = new Mock<IInterceptor>();

            interceptorMock.Setup(i => i.Intercept(It.IsAny<IInvocation>()))
                .Callback((IInvocation invocation) => 
                    {
                        var prop = invocation.TargetType.GetProperty("Date");

                        var fieldAttrs = Attribute.GetCustomAttributes(prop, typeof(FieldAttribute));

                        Assert.That(fieldAttrs.Length, Is.Not.EqualTo(0));

                        invocation.Proceed();
                    });

            var customItem = generator.CreateClassProxy<Holiday>(interceptorMock.Object);         
            var typeOfCustom = customItem.GetType();

            var date = customItem.Date;

            var prop2 = typeOfCustom.GetProperty("Date");

            var fieldAttrs2 = Attribute.GetCustomAttributes(prop2, typeof(FieldAttribute));

            Assert.That(fieldAttrs2.Length, Is.Not.EqualTo(0));
        }
    }
}