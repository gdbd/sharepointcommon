using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using Microsoft.SharePoint;
using SharepointCommon.Interception;

namespace SharepointCommon.Common
{
    internal class UserIterator : IEnumerable<User>
    {
        private readonly SPFieldUserValueCollection _users;

        private readonly ProxyGenerator _proxyGenerator;

        public UserIterator(SPFieldUserValueCollection users)
        {
            _users = users;
            _proxyGenerator = new ProxyGenerator();
        }

        public IEnumerator<User> GetEnumerator()
        {
            if (_users == null) return new List<User>().GetEnumerator();

            return _users
                .Select(u =>
                {
                    if (u.User == null) return _proxyGenerator.CreateClassProxy<User>(new UserAccessInterceptor(u));
                    return _proxyGenerator.CreateClassProxy<Person>(new UserAccessInterceptor(u));
                })
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
