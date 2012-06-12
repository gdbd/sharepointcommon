namespace SharepointCommon.Common.Interceptors
{
    using Castle.DynamicProxy;

    using Microsoft.SharePoint;

    using SharepointCommon.Exceptions;

    internal class UserAccessInterceptor : IInterceptor
    {
        private readonly SPUser _user;

        public UserAccessInterceptor(SPUser user)
        {
            _user = user;
        }

        public void Intercept(IInvocation invocation)
        {
            switch (invocation.Method.Name)
            {
                case "get_Id":
                    invocation.ReturnValue = _user.ID;
                    return;

                case "get_Name":
                    invocation.ReturnValue = _user.Name;
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
