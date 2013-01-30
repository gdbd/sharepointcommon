namespace SharepointCommon.Common.Interceptors
{
    using System.Collections.Generic;

    using Castle.DynamicProxy;

    using Microsoft.SharePoint;

    using SharepointCommon.Attributes;
    using SharepointCommon.Impl;

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
                    invocation.ReturnValue = new QueryList<Item>(_listItem.ParentList);
                    return;
                }

                if (propName.Equals("ListItem"))
                {
                    invocation.ReturnValue = _listItem;
                    return;
                }

                var nomapAttrs = prop.GetCustomAttributes(typeof(NotFieldAttribute), false);
                if (nomapAttrs.Length != 0)
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
