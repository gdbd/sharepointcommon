using Castle.DynamicProxy;
using Microsoft.SharePoint;

namespace SharepointCommon.Interception
{
    internal class UserAccessInterceptor : IInterceptor
    {
        private readonly SPUser _user;
        private SPFieldUserValue _userValue;

        public UserAccessInterceptor(SPUser user)
        {
            _user = user;
        }

        public UserAccessInterceptor(SPFieldUserValue userValue)
        {
            _userValue = userValue;
            _user = userValue.User;
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

                case "GetHashCode":
                    invocation.Proceed();
                    return;

                case "Equals":
                    System.Diagnostics.Debug.Assert(invocation.Arguments.Length > 0, "Argument expected");
                    var anotherUser = invocation.Arguments[0] as User;
                    System.Diagnostics.Debug.Assert(anotherUser != null, "Argument must be instance of User");
                    if (_user != null)
                    {
                        invocation.ReturnValue = anotherUser as Person != null && ((Person)anotherUser).Login.Equals(_user.LoginName);
                        return;
                    }
                    if (_userValue != null)
                    {
                        invocation.ReturnValue = anotherUser != null && _userValue.LookupValue.Equals(anotherUser.Name);
                        return;
                    }
                    
                    throw new SharepointCommonException("both _user and _userValue are null. Equals failed");

                default:
                    throw new SharepointCommonException("UserAccessInterceptor default case. Method Name: " + invocation.Method.Name);
            }
        }
    }
}
