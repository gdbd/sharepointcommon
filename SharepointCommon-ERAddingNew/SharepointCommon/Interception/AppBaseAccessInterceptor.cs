using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using SharepointCommon.Attributes;
using SharepointCommon.Common;

namespace SharepointCommon.Interception
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

            Type listType;
            var isRepository = itemType.Length == 0;
            if (isRepository)
            {
                // creating of a repository class inherited from ListBase<>
                listType = invocation.Method.ReturnType;
            }
            else
            {
                // creating ListBase
                listType = typeof (ListBase<>).MakeGenericType(itemType);
            }

            var splist = GetSpList(invocation.Method);
            string cacheKey = splist.ID.ToString();
            if (_listsCache.ContainsKey(cacheKey))
            {
                return _listsCache[cacheKey];
            }

            object list;
            if (isRepository)
            {
                list = Activator.CreateInstance(listType);
                var listProp = listType.GetProperty("List");
                var webProp = listType.GetProperty("ParentWeb");
                listProp.SetValue(list, splist, null);
                webProp.SetValue(list, _queryWeb, null);
            }
            else
            {
                list = Activator.CreateInstance(listType,
                    BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { splist, _queryWeb }, null);
            }

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
                return _queryWeb.Web.Lists[new Guid(listAttr.Id)];
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
