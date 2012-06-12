namespace SharepointCommon.Test
{
    using NUnit.Framework;

    using SharepointCommon.Common;
    using SharepointCommon.Exceptions;
    using SharepointCommon.Test.Entity;

    using Assert = NUnit.Framework.Assert;

    [TestFixture]
    public class EntityMapperTests
    {
        [Test]
        public void CheckThatPropertyVirtualTest_Throws_On_NoVirtual()
        {
            var noVirtualGetProp = typeof(CustomItem).GetProperty("CustomBoolean");
            Assert.Throws<SharepointCommonException>(
                () => EntityMapper.CheckThatPropertyVirtual(noVirtualGetProp));
        }

        [Test]
        public void CheckThatPropertyVirtualTest_Not_Throws_On_Virtual()
        {
            var virtualGetSetProp = typeof(CustomItem).GetProperty("CustomUser");
            Assert.DoesNotThrow(() => EntityMapper.CheckThatPropertyVirtual(virtualGetSetProp));

            var virtualGetProp = typeof(CustomItem).GetProperty("Author");
            Assert.DoesNotThrow(() => EntityMapper.CheckThatPropertyVirtual(virtualGetProp));
        }
    }
}
