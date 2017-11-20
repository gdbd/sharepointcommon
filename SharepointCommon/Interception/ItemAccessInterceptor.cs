using System;
using System.Collections;
using System.Collections.Generic;
using Castle.DynamicProxy;
using Microsoft.SharePoint;
using SharepointCommon.Common;
using SharepointCommon.Impl;

namespace SharepointCommon.Interception
{
    internal class ItemAccessInterceptor : IInterceptor
    {
        //these three fields initialized only for lookup items
        private readonly int _itemId;
        private readonly string _itemTitle;
        private readonly SPList _parentList;
        
     
        private SPListItem _listItem;
        private readonly bool _reloadLookupItem;
        private List<string> _changedFields = new List<string>();

        internal SPListItem ListItem { get { return _listItem; } }

        public ItemAccessInterceptor(SPListItem listItem, bool reloadLookupItem = true)
        {
            _listItem = listItem;
            _reloadLookupItem = reloadLookupItem;
        }

        public ItemAccessInterceptor(int itemId, string itemTitle, SPList parentList, bool reloadLookupItem = true)
        {
            _itemId = itemId;
            _itemTitle = itemTitle;
            _parentList = parentList;
            _reloadLookupItem = reloadLookupItem;
        }
        
        
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name.StartsWith("set_"))
            {
                var methodName = invocation.Method.Name.Substring(4);
                _changedFields.Add(methodName);
                invocation.Proceed();
                return;
            }

            if (!invocation.Method.Name.StartsWith("get_"))
            {
                Assert.Inconsistent();
            }

            string propName = invocation.Method.Name.Substring(4);

            if (_changedFields.Contains(propName))
            {
                invocation.Proceed();
                return;
            }

            switch (invocation.Method.Name)
            {
                case "get_Id":
                    if (_itemId != 0)
                    {
                        invocation.ReturnValue = _itemId;
                        return;
                    }
                    break;

                case "get_Title":
                    if (!string.IsNullOrEmpty(_itemTitle))//empty string in '..ing' event receivers
                    {
                        invocation.ReturnValue = _itemTitle;
                        return;
                    }
                    break;

                case "get_ParentWeb":
                    LoadListItem();
                    invocation.ReturnValue = new QueryWeb(_listItem.ParentList.ParentWeb);
                    return;

                case "get_ParentList":
                    LoadListItem();
                    var qWeb = new QueryWeb(_listItem.ParentList.ParentWeb);
                    invocation.ReturnValue = qWeb.GetById<Item>(_listItem.ParentList.ID);
                    return;

                case "get_ConcreteParentList":
                    LoadListItem();
                    var qWeb1 = new QueryWeb(_listItem.ParentList.ParentWeb);
                    invocation.ReturnValue = CommonHelper.MakeParentList(invocation.TargetType, qWeb1,
                        _listItem.ParentList.ID);
                    return;

                case "get_ListItem":
                    LoadListItem();
                    invocation.ReturnValue = _listItem;
                    return;

                case "get_Folder":
                    LoadListItem();
                    string folderUrl = _listItem.Url;
                    folderUrl = folderUrl.Replace(_listItem.ParentList.RootFolder.Url + "/", string.Empty);
                    var linkFileName = (string)_listItem[SPBuiltInFieldId.LinkFilename];
                    folderUrl = folderUrl.Replace(linkFileName, string.Empty);
                    invocation.ReturnValue = folderUrl.TrimEnd('/');
                    return;
            }
           
            var prop = invocation.TargetType.GetProperty(propName);

            if (CommonHelper.IsPropertyNotMapped(prop))
            {
                // skip props with [NotField] attribute
                invocation.Proceed();
                return;
            }

            LoadListItem();
            var value = EntityMapper.ToEntityField(prop, _listItem, reloadLookupItem: _reloadLookupItem);

            if (value is Item)
            {
                // for deleted lookup item we need return null
                // it can be found by empty Title
                // see the test: Get_Deleted_Lookup_Value_Returns_Null_Test
                try
                {
                    var t = ((Item)value).Title;
                }
                catch (NullReferenceException)
                {
                    value = null;
                }
            }

            invocation.ReturnValue = value;
        }

        private void LoadListItem()
        {
            if(_listItem != null) return;
            _listItem = _parentList.TryGetItemById(_itemId);
        }
    }
}
