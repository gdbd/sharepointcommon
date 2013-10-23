using System;
using System.Linq.Expressions;
using System.Reflection;
using SharepointCommon.Attributes;
using SharepointCommon.Common;
using SharepointCommon.Entities;
using SharepointCommon.Expressions;
using SharepointCommon.Impl;

// ReSharper disable once CheckNamespace
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

        public TList EnsureList<TList>(Expression<Func<T, TList>> listSelector)
        {
            var visitor = new MemberAccessVisitor();
            var selectedPropertyName = visitor.GetMemberName(listSelector);
            var selectedProperty = typeof(T).GetProperty(selectedPropertyName);
            if (selectedProperty == null) throw new SharepointCommonException(string.Format("property {0} not found!", selectedPropertyName));
            var propertyType = selectedProperty.PropertyType;

            var listAttribute = (ListAttribute)Attribute.GetCustomAttribute(selectedProperty, typeof(ListAttribute));
            if (listAttribute != null && listAttribute.Id != null)
            {
                throw new SharepointCommonException("Cannot ensure list with ID. It may cause mapping problems.");
            }

            if (CommonHelper.ImplementsOpenGenericInterface(propertyType, typeof(IQueryList<>)))
            {
                var entityType = propertyType.GetGenericArguments()[0];
                
                if (listAttribute != null && !string.IsNullOrEmpty(listAttribute.Name))
                {
                    if (QueryWeb.ExistsByName(listAttribute.Name))
                    {
                       return (TList)((QueryWeb)QueryWeb).GetByName(entityType, listAttribute.Name);
                    }

                    var list = ((QueryWeb)QueryWeb).Create(entityType, selectedPropertyName);
                    var titleProp = list.GetType().GetProperty("Title");
                    titleProp.SetValue(list, listAttribute.Name, null);
                    return (TList)list;
                }
                else if (listAttribute != null && !string.IsNullOrEmpty(listAttribute.Url))
                {
                    if (QueryWeb.ExistsByUrl(listAttribute.Url))
                    {
                        return (TList)((QueryWeb)QueryWeb).GetByUrl(entityType, listAttribute.Url);
                    }

                    var list = ((QueryWeb)QueryWeb).Create(entityType, selectedPropertyName);
                    return (TList)list;
                }
                else
                {
                    throw new SharepointCommonException("default case");
                }
                
            }
            else
            {
                throw new SharepointCommonException("Ensure not IQueryList<> not implemented yet!");
            }
        }
    }
}