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
                case "get_ParentList":
                    var qWeb = new QueryWeb(_listItem.ParentList.ParentWeb);
                    invocation.ReturnValue = qWeb.GetById<Item>(_listItem.ParentList.ID);
                    return;

                case "get_ConcreteParentList":
                    var qWeb1 = new QueryWeb(_listItem.ParentList.ParentWeb);
                    invocation.ReturnValue = CommonHelper.MakeParentList(invocation.TargetType, qWeb1,
                        _listItem.ParentList.ID);
                    return;

                case "get_ListItem":
                    invocation.ReturnValue = _listItem;
                    return;

                case "get_Folder":
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

            var value = EntityMapper.ToEntityField(prop, _listItem);

            invocation.ReturnValue = value;
        }
    }
}
