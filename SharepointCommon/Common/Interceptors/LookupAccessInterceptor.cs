namespace SharepointCommon.Common.Interceptors
{
    using System;
    using System.Collections.Generic;

    using Castle.DynamicProxy;

    using Microsoft.SharePoint;

    internal sealed class LookupAccessInterceptor : IInterceptor
    {
        private readonly string _webUrl;
        private readonly SPFieldLookup _fieldLookup;
        private readonly object _fieldValue;
        private readonly SPListItem _listItem;
        private List<string> _changedFields;

        public LookupAccessInterceptor(SPListItem listItem)
        {
            _listItem = listItem;
            _changedFields = new List<string>();
        }

        public LookupAccessInterceptor(string webUrl, SPFieldLookup fieldLookup, object fieldValue)
        {
            _webUrl = webUrl;
            _fieldLookup = fieldLookup;
            _fieldValue = fieldValue;
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
                    var listItem = GetListItem();
                    var ft = FieldMapper.ToFieldType(invocation.Method);
                    var lookupField = listItem.Fields.TryGetFieldByStaticName(ft.Name) as SPFieldLookup;
                    Assert.That(lookupField != null);
                    var lookupItem = GetLookupItem(
                        listItem.Web.Url,
                        new Guid(lookupField.LookupList),
                        listItem[ft.Name]);

                    invocation.ReturnValue = lookupItem == null
                        ? null
                        : EntityMapper.ToEntity(invocation.Method.ReturnType, lookupItem);
                    return;
                }
            }
            invocation.Proceed();
        }

        private SPListItem GetListItem()
        {
            if (_listItem != null)
            {
                // Reload item, because it may been changed before lazy load requested
                using (var wf = WebFactory.Open(_listItem.Web.Url))
                {
                    var list = wf.Web.Lists[_listItem.ParentList.ID];
                    return list.GetItemById(_listItem.ID);
                }
            }
            return GetLookupItem(_webUrl, new Guid(_fieldLookup.LookupList), _fieldValue);
        }

        private SPListItem GetLookupItem(string webUrl, Guid lookupList, object fieldValue)
        {
            using (var wf = WebFactory.Open(webUrl))
            {
                try
                {
                    var list = wf.Web.Lists[lookupList];
                    var lkpValue = new SPFieldLookupValue((string)fieldValue ?? string.Empty);
                    if (lkpValue.LookupId == 0) return null;
                    return list.GetItemById(lkpValue.LookupId);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}