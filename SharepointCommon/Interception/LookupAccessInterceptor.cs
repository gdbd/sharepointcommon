using System.Collections;
using System.Reflection;
using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using Microsoft.SharePoint;
using SharepointCommon.Common;
using SharepointCommon.Impl;

namespace SharepointCommon.Interception
{
    internal sealed class LookupAccessInterceptor : IInterceptor
    {
        private readonly SPListItem _listItem;
        private readonly bool _reloadLookupItem;
        private List<string> _changedFields;
        private readonly object _value;
        private readonly SPList _list;

        public LookupAccessInterceptor(SPListItem listItem, bool reloadLookupItem = true)
        {
            _listItem = listItem;
            _reloadLookupItem = reloadLookupItem;
            _changedFields = new List<string>();
        }

        public LookupAccessInterceptor(object value, SPList list)
        {
            _value = value;
            _list = list;
            _changedFields = new List<string>();
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name.StartsWith("set_"))
            {
                _changedFields.Add(invocation.Method.Name.Substring(4));
                invocation.Proceed();
                return;
            }
            if (invocation.Method.Name.StartsWith("get_"))
            {
                if (_changedFields.Contains(invocation.Method.Name.Substring(4)))
                {
                    invocation.Proceed();
                    return;
                }

                if (typeof(Item).IsAssignableFrom(invocation.Method.ReturnType))
                {
                    invocation.ReturnValue = GetLookupItem(invocation.Method);
                    return;
                }
            }
            invocation.Proceed();
        }

        private object GetLookupItem(MethodInfo memberInfo)
        {
            if (_listItem != null)
            {
                var web = _listItem.ParentList.ParentWeb;
                SPListItem listItem = _listItem;

                if (_reloadLookupItem)
                {
                    
                    var list = web.Lists[_listItem.ParentList.ID];
                    listItem = list.GetItemById(_listItem.ID);
                }
                

                var ft = FieldMapper.ToFieldType(memberInfo);
                var lookupField = listItem.Fields.TryGetFieldByStaticName(ft.Name) as SPFieldLookup;

                if (lookupField == null)
                {
                    throw new SharepointCommonException(string.Format("cant find '{0}' field in list '{1}'", ft.Name,
                        listItem.ParentList.Title));
                }

                var lookupList = web.Lists[new Guid(lookupField.LookupList)];

                // Lookup with picker (ilovesharepoint) returns SPFieldLookupValue
                var fieldValue = listItem[ft.Name];
                var lkpValue = fieldValue as SPFieldLookupValue ??
                               new SPFieldLookupValue((string) fieldValue ?? string.Empty);
                if (lkpValue.LookupId == 0) return null;
                var lookupItem = lookupList.GetItemById(lkpValue.LookupId);

                return lookupItem == null
                    ? null
                    : EntityMapper.ToEntity(memberInfo.ReturnType, lookupItem);
            }
            else
            {
                var wf = new QueryWeb(_list.ParentWeb);
                var list = wf.Web.Lists[_list.ID];
                

                var ft = FieldMapper.ToFieldType(memberInfo);
                var lookupField = list.Fields.TryGetFieldByStaticName(ft.Name) as SPFieldLookup;

                if (lookupField == null)
                {
                    throw new SharepointCommonException(string.Format("cant find '{0}' field in list '{1}'", ft.Name,
                        list.Title));
                }

                var lookupList = wf.Web.Lists[new Guid(lookupField.LookupList)];

                // Lookup with picker (ilovesharepoint) returns SPFieldLookupValue
                var fieldValue = _value;
                var lkpValue = fieldValue as SPFieldLookupValue ??
                               new SPFieldLookupValue((string)fieldValue ?? string.Empty);
                if (lkpValue.LookupId == 0) return null;
                var lookupItem = lookupList.GetItemById(lkpValue.LookupId);

                return lookupItem == null
                    ? null
                    : EntityMapper.ToEntity(memberInfo.ReturnType, lookupItem);
            }
        }
    }
}