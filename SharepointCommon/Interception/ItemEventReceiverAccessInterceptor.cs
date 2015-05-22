using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using Microsoft.SharePoint;
using Microsoft.SharePoint.JSGrid;
using Microsoft.SharePoint.Utilities;
using SharepointCommon.Attributes;
using SharepointCommon.Common;
using SharepointCommon.Impl;

namespace SharepointCommon.Interception
{
    internal class ItemEventReceiverAccessInterceptor : IInterceptor
    {
        private SPList _list;
        private readonly Hashtable _afterProperties;

        
        public ItemEventReceiverAccessInterceptor(SPList list, Hashtable afterProperties)
        {
            _list = list;
            _afterProperties = afterProperties;
        }

        public void Intercept(IInvocation invocation)
        {
            var propName = invocation.Method.Name.Substring(4);
            var prop = invocation.TargetType.GetProperty(propName);
            
            var fieldAttrs = prop.GetCustomAttributes(typeof(FieldAttribute), true);

            string spPropName;
            if (fieldAttrs.Length != 0)
            {
                spPropName = ((FieldAttribute)fieldAttrs[0]).Name ?? propName;
            }
            else
            {
                spPropName = FieldMapper.TranslateToFieldName(propName);
            }
            propName = spPropName;
            if (invocation.Method.Name.StartsWith("set_"))
            {
                throw new SharepointCommonException("Set operations on AfterProperties mapped item not implemented yet!");
            }

            if (!invocation.Method.Name.StartsWith("get_"))
            {
                Assert.Inconsistent();
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
                    invocation.ReturnValue = null;
                    return;
                }

                case "get_Folder":
                {
                    invocation.ReturnValue = null;
                    return;
                }
            }

            if (CommonHelper.IsPropertyNotMapped(prop))
            {
                // skip props with [NotField] attribute
                invocation.Proceed();
                return;
            }

            if (_afterProperties.ContainsKey(propName))
            {
                var field = _list.Fields.GetFieldByInternalName(propName);

                var val = EntityMapper.ToEntityField(prop, null, field: field, value: _afterProperties[propName], reloadLookupItem: true);
                
                invocation.ReturnValue = val;
                return;
            }
            else if (_afterProperties.ContainsKey("vti_" + propName.ToLower()))
            {
                var field = _list.Fields.GetFieldByInternalName(propName);

                var val = EntityMapper.ToEntityField(prop, null, field: field, value: _afterProperties["vti_" + propName.ToLower()], reloadLookupItem: true);

                invocation.ReturnValue = val;
                return;
            }
            else
            {
                var defaultValue = CommonHelper.GetDefaultValue(invocation.Method.ReturnType);
                invocation.ReturnValue = defaultValue;
                return;
            }


           
        }
         
    }
}
