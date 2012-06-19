namespace SharepointCommon.Common.Interceptors
{
    using Castle.DynamicProxy;

    using Microsoft.SharePoint;

    using SharepointCommon.Impl;

    public class ItemAccessInterceptor : IInterceptor
    {
        private readonly SPListItem _listItem;

        public ItemAccessInterceptor(SPListItem listItem)
        {
            _listItem = listItem;
        }

        public void Intercept(IInvocation invocation)
        {
            switch (invocation.Method.Name)
            {
                case "get_ParentList":
                    invocation.ReturnValue = new QueryList<Item>(_listItem.ParentList);
                    return;
            }
            invocation.Proceed();
        }
    }
}
