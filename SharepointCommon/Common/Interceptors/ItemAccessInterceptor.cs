using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using Microsoft.SharePoint;
using SharepointCommon.Attributes;
using SharepointCommon.Impl;

namespace SharepointCommon.Common.Interceptors
{
    internal class ItemAccessInterceptor : IInterceptor
    {
        private readonly SPListItem _listItem;
        private List<string> _changedFields;

        public ItemAccessInterceptor(SPListItem listItem)
        {
            _listItem = listItem;
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

                string propName = invocation.Method.Name.Substring(4);
                var prop = invocation.TargetType.GetProperty(propName);

                if (propName.Equals("ParentList"))
                {
                    var qWeb = new QueryWeb(_listItem.ParentList.ParentWeb);
                    invocation.ReturnValue = qWeb.GetById<Item>(_listItem.ParentList.ID);
                    return;
                }

                if (propName.Equals("ListItem"))
                {
                    invocation.ReturnValue = _listItem;
                    return;
                }
                
                if (CommonHelper.IsPropertyNotMapped(prop))
                {
                    // skip props with [NotField] attribute
                    invocation.Proceed();
                    return;
                }

                var value = EntityMapper.ToEntityField(prop, _listItem);

                invocation.ReturnValue = value;
                return;
            }
            
            Assert.Inconsistent();
        }
    }
}
