using Moq;

namespace SharepointCommon.Test
{
    using NUnit.Framework;

    public class MoqLibraryTests
    {
        [Test]
        public void Moq_QueryWeb_Test()
        {
            var moqQueryWeb = new Mock<IQueryWeb>();
            var moqQueryList = new Mock<IQueryList<Item>>(MockBehavior.Strict);

            moqQueryWeb.Setup(m => m.GetByName<Item>(It.IsAny<string>()))
                .Returns(moqQueryList.Object);

            moqQueryList.Setup(m => m.Add(It.IsAny<Item>()));

            IQueryWeb queryWeb = moqQueryWeb.Object;

            IQueryList<Item> list = queryWeb.GetByName<Item>("Test");

            list.Add(new Item());
        }
    }
}