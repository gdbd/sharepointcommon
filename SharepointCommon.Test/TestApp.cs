using SharepointCommon.Attributes;

namespace SharepointCommon.Test
{
    public class TestApp : AppBase<TestApp>
    {
        [List(Id = "8A083287-CAEF-4DFA-8246-E8236676F5A1")]
        public virtual IQueryList<Item> Test { get; set; }

        [NotMapped]
        public string Test2 { get; set; }

        public void SetListThatMustThrows()
        {
            var list = UserInfoList;
            UserInfoList = list;
        }

        public void SetListThatMustNoThrows()
        {
            Test2 = "asd";
        }
    }

    public class TestAppNoVirtualProperty : AppBase<TestAppNoVirtualProperty>
    {
        [List(Id = "8A083287-CAEF-4DFA-8246-E8236676F5A1")]
        public IQueryList<Item> Test { get; set; }
    }
}