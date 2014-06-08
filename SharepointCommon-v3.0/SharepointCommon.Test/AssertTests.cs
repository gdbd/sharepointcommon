using NUnit.Framework;
using SharepointCommon.Common;
using SharepointCommon.Test.Entity;
using NuAssert = NUnit.Framework.Assert;
using ShpcAssert = SharepointCommon.Common.Assert;


namespace SharepointCommon.Test
{
    [TestFixture]
    public class AssertTests
    {
        [Test]
        public void CheckThatPropertyVirtualTest_Throws_On_NoVirtual()
        {
            var noVirtualGetProp = typeof(CustomItemNoVirtualProperty).GetProperty("CustomField1");
            NuAssert.Throws<SharepointCommonException>(
                () => ShpcAssert.IsPropertyVirtual(noVirtualGetProp));
        }

        [Test]
        public void CheckThatPropertyVirtualTest_Not_Throws_On_Virtual()
        {
            var virtualGetSetProp = typeof(CustomItem).GetProperty("CustomUser");
            NuAssert.DoesNotThrow(() => ShpcAssert.IsPropertyVirtual(virtualGetSetProp));

            var virtualGetProp = typeof(CustomItem).GetProperty("Author");
            NuAssert.DoesNotThrow(() => ShpcAssert.IsPropertyVirtual(virtualGetProp));
        }
    }
}
