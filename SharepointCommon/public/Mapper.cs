using Microsoft.SharePoint;
using SharepointCommon.Common;

// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    /// <summary>
    /// Allow perform listitem to entity mapping
    /// </summary>
    public static class Mapper
    {
        /// <summary>
        /// Map SPListItem to SharePointCommon.Item by one row of code
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listItem"></param>
        /// <returns></returns>
        public static T ToEntity<T>(SPListItem listItem) where T : Item, new()
        {
            return EntityMapper.ToEntity<T>(listItem);
        }
    }
}
