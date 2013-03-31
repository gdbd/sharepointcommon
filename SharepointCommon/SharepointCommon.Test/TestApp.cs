namespace SharepointCommon.Test
{
    public class TestApp : AppBase<TestApp>
    {
        public void SetListThatMustThrows()
        {
            var list = UserInfoList;
            UserInfoList = list;
        }
    }
}