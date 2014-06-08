using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Application
{
    public class TestAppNoVirtualProperty : AppBase<TestAppNoVirtualProperty>
    {
        [List(Id = "8A083287-CAEF-4DFA-8246-E8236676F5A1")]
        public IQueryList<Item> Test { get; set; }
    }
}