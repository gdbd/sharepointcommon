using System;
using SharepointCommon.Attributes;
using SharepointCommon.Common;
using SharepointCommon.Entities;
using SharepointCommon.Impl;

namespace SharepointCommon
{
    /// <summary>
    /// Base class for derive 'Application' objects from it.
    /// By inherit class from 'AppBase' you gain possibility
    /// to map auto-properties of list wrappers to SharePoint lists
    /// </summary>
    ///  
    /// <example> 
    /// <![CDATA[
    /// public class TestApp : AppBase<TestApp>
    /// { 
    ///     [List(Url="lists/contract")]
    ///     public virtual IQueryList<Contract> Contracts { get; }
    /// }
    /// 
    /// using (var app01 = TestApp.Factory.OpenNew(_webUrl))
    /// {
    ///     var contractsList = app01.Contracts;
    /// }
    /// ]]>
    /// </example> 
    /// <typeparam name="T">Type of your class derived from AppBase</typeparam>
    public class AppBase<T> : IDisposable where T : AppBase<T>
    {
        static AppBase()
        {
            Factory = new AppFac<T>();
        }

        /// <summary>
        /// Gets factory instance for creating
        /// <see cref="AppBase"/> derived objects
        /// </summary>
        [NotMapped]
        public static IAppFac<T> Factory { get; internal set; }

        /// <summary>
        /// Gets underlying <see cref="IQueryWeb"/> object
        /// </summary>
        [NotMapped]
        public IQueryWeb QueryWeb { get; internal set; }

        /// <summary>
        /// Instance of IQueryList associated with SharePoint 'User Information List' list
        /// </summary>
        [List(Url = "_catalogs/users")]
        public virtual IQueryList<UserInfoList> UserInfoList { get; set; }

        /// <summary>
        /// If true, call Dispose method on <see cref="QueryWeb"/> when disposing itself
        /// </summary>
        protected internal bool ShouldDispose { get; set; }

        public virtual void Dispose()
        {
            if (ShouldDispose)
            {
                QueryWeb.Dispose();
            }
        }

        /// <summary>
        /// Method that called after object initialization complete
        /// </summary>
        public virtual void Init()
        {
        }
    }
}