using System;
using System.Linq;
using System.Linq.Expressions;
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

        public TList Ensure<TList>(Expression<Func<T, TList>> listSelector)
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

            if (!CommonHelper.ImplementsOpenGenericInterface(propertyType, typeof (IQueryList<>)))
            {
                throw new SharepointCommonException("Ensure not IQueryList<> not supported yet!");
            }
            
            var args = propertyType.GetGenericArguments();
            var isRepository = args.Length == 0;
            Type entityType;
            if (isRepository)
            {
                entityType = GetRepositoryGenericArgument(propertyType);
            }
            else
            {
                entityType = args[0];
            }

            if (listAttribute != null && !string.IsNullOrEmpty(listAttribute.Name))
            {
                if (QueryWeb.ExistsByName(listAttribute.Name))
                {
                    var qList = ((QueryWeb) QueryWeb).GetByName(entityType, listAttribute.Name);
                    if (!isRepository)
                    {
                        // existing list
                        return (TList) qList;
                    }
                    else
                    {
                        //existing repository
                        var splistProp1 = qList.GetType().GetProperty("List");
                        var splist1 = splistProp1.GetValue(qList, null);
                        return CreateRepository<TList>(propertyType, splist1);
                    }
                }

                var list = ((QueryWeb) QueryWeb).Create(entityType, selectedPropertyName);
                var splist = list.GetType().GetProperty("List").GetValue(list, null);

                list.GetType().GetProperty("Title").SetValue(list, listAttribute.Name, null);

           
                if (isRepository)
                {
                    //new repository
                    return CreateRepository<TList>(propertyType, splist);
                }
                else
                {
                    //new list
                    return (TList) list;
                }
            }
            else if (listAttribute != null && listAttribute.Url != null)
            {
                var nameForCreateCorrectUrl = listAttribute.Url.Substring(
                    listAttribute.Url.LastIndexOf("/", StringComparison.Ordinal) + 1);

                if (QueryWeb.ExistsByUrl(listAttribute.Url))
                {
                    var existingList = ((QueryWeb)QueryWeb).GetByUrl(entityType, listAttribute.Url);

                    if (!isRepository)
                    {
                        // existing list
                        return (TList)existingList;
                    }
                    else
                    {
                        // existing repository
                        var splist1 = existingList.GetType().GetProperty("List").GetValue(existingList, null);
                        return CreateRepository<TList>(propertyType, splist1);
                    }
                }

                var list = ((QueryWeb) QueryWeb).Create(entityType, nameForCreateCorrectUrl);
                var splist = list.GetType().GetProperty("List").GetValue(list, null);
                list.GetType().GetProperty("Title").SetValue(list, selectedPropertyName, null);
                
                if (isRepository)
                {
                    // new repository
                    return CreateRepository<TList>(propertyType, splist);
                }
                else
                {
                    // new list
                    return (TList) list;
                }
            }

            throw new SharepointCommonException("default case.");
            
        }

        private static Type GetRepositoryGenericArgument(Type type)
        {
            var ret = type;
            while (ret != null && ret.Name != "ListBase`1")
            {
                ret = ret.BaseType;
            }

            if (ret == null)
            {
                throw new SharepointCommonException(string.Format("cannot find ListBase parent for repository {0}",
                    type.Name));
            }

            return ret.GetGenericArguments().First();
        }

        private TList CreateRepository<TList>(Type propertyType, object splist)
        {
            var list1 = Activator.CreateInstance(propertyType);
            var listProp = propertyType.GetProperty("List");
            var webProp = propertyType.GetProperty("ParentWeb");
            listProp.SetValue(list1, splist, null);
            webProp.SetValue(list1, QueryWeb, null);
            return (TList)list1;
        }
    }
}