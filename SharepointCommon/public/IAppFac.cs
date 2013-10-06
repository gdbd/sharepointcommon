using System;

// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    /// <summary>
    /// Factory for produce 'AppBase' derived objects
    /// </summary>
    /// <typeparam name="T">Type of class derived from AppBase</typeparam>
    public interface IAppFac<T> where T : AppBase<T>
    {
        /// <summary>
        /// Create object based on existing <see cref="IQueryWeb"/> queryWeb
        /// </summary>
        /// <returns>AppBase derived object of type T</returns>
        T ExistingWeb(IQueryWeb queryWeb);

        /// <summary>
        /// Create object based on existing <see cref="SPWeb"/> queryWeb
        /// </summary>
        /// <returns>AppBase derived object of type T</returns>
        T ExistingWeb(Microsoft.SharePoint.SPWeb spWeb);
        
        /// <summary>
        /// Create object based on SPContext.Current
        /// </summary>
        /// <returns>AppBase derived object of type T</returns>
        T CurrentContext();

        /// <summary>
        /// Create object based on SPContext.Current with elevate permissions
        /// </summary>
        /// <returns>AppBase derived object of type T</returns>
        T ElevatedFromCurrentContext();

        /// <summary>
        /// Create new object by site url. Must call Dispose method or be in using clause
        /// </summary>
        /// <returns>AppBase derived object of type T</returns>
        T OpenNew(string webUrl);

        /// <summary>
        /// Create new object by site id and web id. Must call Dispose method or be in using clause
        /// </summary>
        /// <returns>AppBase derived object of type T</returns>
        T OpenNew(Guid siteId, Guid webId);

        /// <summary>
        /// Create new object in elevated mode by site url. Must call Dispose method or be in using clause
        /// </summary>
        /// <returns>AppBase derived object of type T</returns>
        T ElevatedNew(string webUrl);
        
        /// <summary>
        /// Create new object by site id and web id in elevated mode. Must call Dispose method or be in using clause
        /// </summary>
        /// <returns>AppBase derived object of type T</returns>
        T ElevatedNew(Guid siteId, Guid webId);
    }
}