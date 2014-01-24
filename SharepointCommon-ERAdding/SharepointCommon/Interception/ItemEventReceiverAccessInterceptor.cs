using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using Microsoft.SharePoint;
using SharepointCommon.Common;
using SharepointCommon.Impl;

namespace SharepointCommon.Interception
{
    internal class ItemEventReceiverAccessInterceptor : IInterceptor
    {
        private SPList _list;
        private Hashtable _mappedProperties;
        private Hashtable _notMappedProperties;
        private List<string> _changedFields;
        private SPListItem _listItem;
        public ItemEventReceiverAccessInterceptor(SPList list, Hashtable properties)
        {
            _list = list;
            _mappedProperties = properties;
            _notMappedProperties = new Hashtable();
            _changedFields = new List<string>();
        }

        public ItemEventReceiverAccessInterceptor(SPListItem list, Hashtable properties)
        {
            foreach (DictionaryEntry property in properties)
            {
                list[property.Key.ToString()] = property.Value;
            }
            _listItem = list;
            _list = list.ParentList;
            _mappedProperties = properties;
            _notMappedProperties = new Hashtable();
            _changedFields = new List<string>();
        }
        public void Intercept(IInvocation invocation)
        {
            var propName = invocation.Method.Name.Substring(4);
            var prop = invocation.TargetType.GetProperty(propName);
            propName = FieldMapper.TranslateToFieldName(propName);
            if (invocation.Method.Name.StartsWith("set_"))
            {
                _changedFields.Add(propName);
                if (prop.GetSetMethod(false) != null)
                {
                    if (_mappedProperties.ContainsKey(propName) && _list.Fields.ContainsField(propName))
                    {
                        _mappedProperties[propName] = invocation.Arguments[0];
                    }
                    else if (!_mappedProperties.ContainsKey(propName) && _list.Fields.ContainsField(propName))
                    {
                        _mappedProperties.Add(propName, invocation.Arguments[0]);
                    }
                }
                else
                {
                    if (_notMappedProperties.ContainsKey(propName) && _list.Fields.ContainsField(propName))
                    {
                        _notMappedProperties[propName] = invocation.Arguments[0];
                    }
                    else if (!_notMappedProperties.ContainsKey(propName) && _list.Fields.ContainsField(propName))
                    {
                        _notMappedProperties.Add(propName, invocation.Arguments[0]);
                    }
                }
                invocation.Proceed();
                return;
            }

            if (!invocation.Method.Name.StartsWith("get_"))
            {
                Assert.Inconsistent();
            }

            if (_changedFields.Contains(propName))
            {
                invocation.Proceed();
                return;
            }
            switch (invocation.Method.Name)
            {
                case "get_ParentList":
                    var qWeb = new QueryWeb(_list.ParentWeb);
                    invocation.ReturnValue = qWeb.GetById<Item>(_list.ID);
                    return;

                case "get_ConcreteParentList":
                    var qWeb1 = new QueryWeb(_list.ParentWeb);
                    invocation.ReturnValue = CommonHelper.MakeParentList(invocation.TargetType, qWeb1,
                        _list.ID);
                    return;

                case "get_ListItem":
                {
                    invocation.ReturnValue = _listItem;
                    return;
                }

                case "get_Folder":
                {
                    if (_list == null)
                        invocation.ReturnValue = null;
                    else
                    {
                        var folderUrl = _listItem.Url;
                        folderUrl = folderUrl.Replace(_listItem.ParentList.RootFolder.Url + "/", string.Empty);
                        var linkFileName = (string) _listItem[SPBuiltInFieldId.LinkFilename];
                        folderUrl = folderUrl.Replace(linkFileName, string.Empty);
                        invocation.ReturnValue = folderUrl.TrimEnd('/');
                    }
                    return;
                }
            }

            
            
            if (CommonHelper.IsPropertyNotMapped(prop))
            {
                // skip props with [NotField] attribute
                invocation.Proceed();
                return;
            }

            object value;


            if (prop.GetSetMethod(false) != null)
            {
                if (!_mappedProperties.ContainsKey(propName) && _list.Fields.ContainsField(propName))
                {
                    _mappedProperties.Add(propName,
                        _listItem == null
                            ? EntityMapper.ToEntityField(prop)
                            : EntityMapper.ToEntityField(prop, _listItem));
                }
                value = _mappedProperties[propName];
                invocation.ReturnValue = value;
                return;
            }
            else
            {
                if (!_notMappedProperties.ContainsKey(propName) && _list.Fields.ContainsField(propName))
                {
                    _notMappedProperties.Add(propName,
                        _listItem == null
                            ? EntityMapper.ToEntityField(prop)
                            : EntityMapper.ToEntityField(prop, _listItem));
                }
                invocation.ReturnValue = _notMappedProperties[propName];
                return;
            }
            invocation.Proceed();
            return;
        }
    }
}
