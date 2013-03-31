using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using SharepointCommon.Attributes;
using SharepointCommon.Impl;

namespace SharepointCommon.Common.Interceptors
{
    internal class AppBaseAccessInterceptor : IInterceptor
    {
        private readonly IQueryWeb _queryWeb;
        private readonly IDictionary<string, object> _listsCache = new Dictionary<string, object>(); 

        public AppBaseAccessInterceptor(IQueryWeb queryWeb)
        {
            _queryWeb = queryWeb;
        }

        public void Intercept(IInvocation invocation)
        {
            if (IsQueryListAccessCall(invocation))
            {
                var list = GetList(invocation);
                invocation.ReturnValue = list;
                return;
            }

            if (IsAttemptSetQueryList(invocation))
            {
                throw new SharepointCommonException("Properties of IQueryList<> cannot be set directly. Use 'ListAttribute' to map list to property.");
            }

            invocation.Proceed();
        }

        private bool IsAttemptSetQueryList(IInvocation invocation)
        {
            return invocation.Method.Name.StartsWith("set_");
        }

        private object GetList(IInvocation invocation)
        {
            var itemType = invocation.Method.ReturnType.GetGenericArguments();
            var listType = typeof(QueryList<>).MakeGenericType(itemType);
            
            var splist = GetSpList(invocation.Method);
            string cacheKey = splist.ID.ToString();
            if (_listsCache.ContainsKey(cacheKey))
            {
                return _listsCache[cacheKey];
            }

            var list = Activator.CreateInstance(listType, splist, _queryWeb);
            _listsCache[cacheKey] = list;
            return list;
        }

        private SPList GetSpList(MethodInfo method)
        {
            string propName = method.Name.Substring(4);
            var prop = method.DeclaringType.GetProperty(propName);

            var listAttr = (ListAttribute)Attribute.GetCustomAttribute(prop, typeof(ListAttribute));
            if (listAttr == null)
            {
                return _queryWeb.Web.Lists[method.Name];
            }

            if (listAttr.Name != null)
            {
                return _queryWeb.Web.Lists[listAttr.Name];
            }

            if (listAttr.Url != null)
            {
                var url = SPUrlUtility.CombineUrl(_queryWeb.Web.ServerRelativeUrl, listAttr.Url);
                return _queryWeb.Web.GetList(url);
            }

            if (listAttr.Id != null)
            {
                return _queryWeb.Web.Lists[listAttr.Id.Value];
            }

            throw new SharepointCommonException(string.Format("Unable find list for property {0}", method.Name));
        }

        private bool IsQueryListAccessCall(IInvocation invocation)
        {
            if (CommonHelper.ImplementsOpenGenericInterface(invocation.Method.ReturnType, typeof(IQueryList<>)))
            {
                return true;
            }

            return false;
        }
    }
}
