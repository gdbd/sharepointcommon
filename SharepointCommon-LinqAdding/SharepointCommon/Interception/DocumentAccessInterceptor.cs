using Microsoft.SharePoint.Utilities;
using SharepointCommon.Entities; 
using Castle.DynamicProxy;
using Microsoft.SharePoint;

namespace SharepointCommon.Interception
{

    internal class DocumentAccessInterceptor : IInterceptor
    {
        private readonly SPListItem _item;

        public DocumentAccessInterceptor(SPListItem item)
        {
            _item = item;
        }

        public void Intercept(IInvocation invocation)
        {
            switch (invocation.Method.Name)
            {
                case "get_Name":
                    invocation.ReturnValue = _item.File.Name;
                    return;

                case "get_Content":
                    invocation.ReturnValue = _item.File.OpenBinary();
                    return;

                case "get_Size":
                    invocation.ReturnValue = _item.File.OpenBinary().LongLength;
                    return;

                case "get_Url":
                    invocation.ReturnValue = SPUrlUtility.CombineUrl(_item.ParentList.ParentWeb.ServerRelativeUrl, _item.File.Url);
                    return;

                case "get_Icon":
                    invocation.ReturnValue = "/_layouts/images/" + _item.File.IconUrl;
                    return;

                case "get_Folder":
                    // check that entity is 'Document'. if not need pass proccssing to 'ItemAccessInterceptor'
                    if (!typeof (Document).IsAssignableFrom(invocation.TargetType))
                    {
                        break;
                    }

                    string folderUrl = _item.Url;
                    folderUrl = folderUrl.Replace(_item.ParentList.RootFolder.Url + "/", string.Empty);
                    folderUrl = folderUrl.Replace("/" + _item.File.Name, string.Empty);

                    // for files placed in root folder, set Folder to null
                    if (folderUrl.Equals(_item.File.Name)) folderUrl = null;

                    invocation.ReturnValue = folderUrl;
                    return;
            }
            invocation.Proceed();
        }
    }
}
