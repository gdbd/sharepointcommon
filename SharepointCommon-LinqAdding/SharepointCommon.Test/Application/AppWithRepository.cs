using SharepointCommon.Attributes;
using SharepointCommon.Test.Repository;

namespace SharepointCommon.Test.Application
{
    public class AppWithRepository : AppBase<AppWithRepository>
    {
        [List(Url = "lists/CustomItems")]
        public virtual TestRepository CustomItems { get; set; }
    }
}
