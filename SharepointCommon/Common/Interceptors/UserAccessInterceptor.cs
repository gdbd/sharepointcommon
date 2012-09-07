namespace SharepointCommon.Common.Interceptors
{
    using Castle.DynamicProxy;

    using Microsoft.SharePoint;

    internal class UserAccessInterceptor : IInterceptor
    {
        private readonly SPUser _user;
        private SPFieldLookupValue _userValue;

        public UserAccessInterceptor(SPUser user)
        {
            _user = user;
        }

        public UserAccessInterceptor(SPFieldLookupValue userValue)
        {
            _userValue = userValue;
        }

        public void Intercept(IInvocation invocation)
        {
            switch (invocation.Method.Name)
            {
                case "get_Id":
                    if (_user != null) invocation.ReturnValue = _user.ID;
                    if (_userValue != null) invocation.ReturnValue = _userValue.LookupId;
                    return;

                case "get_Name":
                    if (_user != null) invocation.ReturnValue = _user.Name;
                    if (_userValue != null) invocation.ReturnValue = _userValue.LookupValue;
                    return;

                case "get_Email":
                    invocation.ReturnValue = _user.Email;
                    return;

                case "get_Login":
                    invocation.ReturnValue = _user.LoginName;
                    return;

                default:
                    throw new SharepointCommonException("UserAccessInterceptor default case.");
            }
        }
    }
}
