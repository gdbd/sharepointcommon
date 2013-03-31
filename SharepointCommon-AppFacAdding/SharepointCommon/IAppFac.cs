using System;

namespace SharepointCommon
{
    public interface IAppFac<T> where T : AppBase<T>
    {
        /// <summary>
        /// Create object based on existing <see cref="IQueryWeb"/> queryWeb
        /// </summary>
        T ExistingWeb(IQueryWeb queryWeb);
        
        /// <summary>
        /// Create object based on <see cref="SPContext.Current.Web"/>
        /// </summary>
        T CurrentContext();

        /// <summary>
        /// Create object based on <see cref="SPContext.Current.Web"/> with elevate permissions
        /// </summary>
        T ElevatedFromCurrentContext();

        /// <summary>
        /// Create new object by site url. Must call Dispose method or be in using clause
        /// </summary>
        T OpenNew(string webUrl);

        /// <summary>
        /// Create new object by site id and web id. Must call Dispose method or be in using clause
        /// </summary>
        T OpenNew(Guid siteId, Guid webId);

        /// <summary>
        /// Create new object in elevated mode by site url. Must call Dispose method or be in using clause
        /// </summary>
        T ElevatedNew(string webUrl);
        
        /// <summary>
        /// Create new object by site id and web id in elevated mode. Must call Dispose method or be in using clause
        /// </summary>
        T ElevatedNew(Guid siteId, Guid webId);
    }
}