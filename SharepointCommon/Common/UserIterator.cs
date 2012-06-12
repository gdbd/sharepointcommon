namespace SharepointCommon.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Castle.DynamicProxy;

    using Microsoft.SharePoint;

    using SharepointCommon.Common.Interceptors;

    internal class UserIterator : IEnumerable<User>
    {
        private readonly IEnumerable<SPUser> _users;

        private readonly ProxyGenerator _proxyGenerator;

        public UserIterator(IEnumerable<SPUser> users)
        {
            _users = users;
            _proxyGenerator = new ProxyGenerator();
        }

        public IEnumerator<User> GetEnumerator()
        {
            if (_users == null) return new List<User>().GetEnumerator();

            return _users
                .Select(u => _proxyGenerator.CreateClassProxy<User>(new UserAccessInterceptor(u)))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
