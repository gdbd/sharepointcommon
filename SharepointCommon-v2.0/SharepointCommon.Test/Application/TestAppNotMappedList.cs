using SharepointCommon.Attributes;
using SharepointCommon.Entities;

namespace SharepointCommon.Test.Application
{
    public class TestAppNotMappedList : AppBase<TestAppNotMappedList>
    {
        [NotMapped]
        public IQueryList<UserInfoList> Test { get; set; }
    }
}