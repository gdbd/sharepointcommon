This page describes how to use SharepointCommon with unit testing and TDD.

# Mock objects

## Mock QueryWeb and QueryList with **Moq**

{{
// create QueryWeb moq
var moqQueryWeb = new Mock<IQueryWeb>();

// create QueryList moq
var moqQueryList = new Mock<IQueryList<Item>>(MockBehavior.Strict);

// setup GetByName method to return moq
moqQueryWeb.Setup(m => m.GetByName<Item>(It.IsAny<string>()))
    .Returns(moqQueryList.Object);            

// TODO: perform additional setup on moqQueryList

IQueryWeb queryWeb = moqQueryWeb.Object;
IQueryList<Item> queryList = queryWeb.GetByName<Item>("Test");

// start using queryWeb and queryList

}}