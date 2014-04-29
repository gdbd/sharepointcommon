using Microsoft.SharePoint;

// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    internal static class ListExtention
    {
        internal static SPListItem TryGetItemById(this SPList list, int id)
        {
            try
            {
                return list.GetItemById(id);
            }
            catch
            {
                return null;   
            }
        }
    }
}
