using System.Linq.Expressions;
using System.Reflection;
using Microsoft.SharePoint.Utilities;
using SharepointCommon.Attributes;
using SharepointCommon.Expressions;
using SharepointCommon.Impl;

namespace SharepointCommon.Common
{
    using System;
    using System.Linq;

    using Microsoft.SharePoint;


    internal sealed class CommonHelper
    {
        internal static SPUser GetUser(SPListItem item, string fieldStaticName)
        {
            var userField = (SPFieldUser)item.Fields.TryGetFieldByStaticName(fieldStaticName);
            if (userField == null) throw new SharepointCommonException(string.Format("Field {0} not exist", fieldStaticName));
            
            var fieldValue = (SPFieldUserValue)userField.GetFieldValue((string)item[fieldStaticName]);

            if (fieldValue == null) return null;

            return fieldValue.User;
        }

        internal static SPUser GetUser(SPList list, string fieldStaticName, object value)
        {
            var userField = (SPFieldUser)list.Fields.TryGetFieldByStaticName(fieldStaticName);
            if (userField == null) throw new SharepointCommonException(string.Format("Field {0} not exist", fieldStaticName));

            var fieldValue = (SPFieldUserValue)userField.GetFieldValue((string)value);

            if (fieldValue == null) return null;

            return fieldValue.User;
        }

        internal static SPFieldUserValueCollection GetUsers(SPList list, string fieldStaticName, object value)
        {
            var userField = (SPFieldUser)list.Fields.TryGetFieldByStaticName(fieldStaticName);
            if (userField == null) throw new SharepointCommonException(string.Format("Field {0} not exist", fieldStaticName));
            var ids = ((string)value).Split(new[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
            var users = new SPFieldUserValueCollection();
            foreach (var id in ids)
            {
                try
                {
                    var user = list.ParentWeb.AllUsers.GetByID(int.Parse(id));
                    users.Add(new SPFieldUserValue(list.ParentWeb, user.ID, user.LoginName));
                }
                catch (SPException)
                {
                    var group = list.ParentWeb.Groups.GetByID(int.Parse(id));
                    users.Add(new SPFieldUserValue(list.ParentWeb, group.ID, group.Name));
                }
            }
            return users;
        }

        internal static SPFieldUserValueCollection GetUsers(SPListItem item, string fieldStaticName)
        {
            var userField = (SPFieldUser)item.Fields.TryGetFieldByStaticName(fieldStaticName);
            if (userField == null) throw new SharepointCommonException(string.Format("Field {0} not exist", fieldStaticName));

            return (SPFieldUserValueCollection)item[fieldStaticName];
        }

        internal static bool IsPropertyNotMapped(PropertyInfo prop)
        {
            // NotFieldAttribute is obsolete but old code can still use it
#pragma warning disable 612,618
            var notFieldAttrs = prop.GetCustomAttributes(typeof(NotFieldAttribute), false);
#pragma warning restore 612,618
            var nomapAttrs = prop.GetCustomAttributes(typeof(NotMappedAttribute), false);

            var attrs = notFieldAttrs.Union(nomapAttrs);

            return attrs.Any();
        }

        /// <summary>Determines whether a type, like IList<int>, implements an open generic interface, like
        /// IEnumerable<>. Note that this only checks against *interfaces*.</summary>
        /// <param name="candidateType">The type to check.</param>
        /// <param name="openGenericInterfaceType">The open generic type which it may impelement</param>
        /// <returns>Whether the candidate type implements the open interface.</returns>
        internal static bool ImplementsOpenGenericInterface(Type candidateType, Type openGenericInterfaceType)
        {
            Assert.NotNull(candidateType);
            Assert.NotNull(openGenericInterfaceType);

            return
                candidateType.Equals(openGenericInterfaceType) ||
                (candidateType.IsGenericType && candidateType.GetGenericTypeDefinition().Equals(openGenericInterfaceType)) ||
                candidateType.GetInterfaces().Any(i => i.IsGenericType && ImplementsOpenGenericInterface(i, openGenericInterfaceType));
        }

        internal static object MakeParentList(Type entityType, IQueryWeb queryWeb, Guid listId)
        {
            var method = typeof(QueryWeb).GetMethod("GetById").MakeGenericMethod(entityType);
            return method.Invoke(queryWeb, new object[] { listId });
        }

        internal static string GetFieldInnerName<T>(Expression<Func<T, object>> fieldSelector) where T : Item, new()
        {
            if (fieldSelector == null) throw new ArgumentNullException("fieldSelector");

            var memberAccessor = new MemberAccessVisitor();
            string propName = memberAccessor.GetMemberName(fieldSelector);

            var prop = typeof(T).GetProperty(propName);

            var fieldAttrs = prop.GetCustomAttributes(typeof(FieldAttribute), true);
            
            if (fieldAttrs.Length != 0)
            {
                var spPropName = ((FieldAttribute)fieldAttrs[0]).Name;
                if (spPropName != null) propName = spPropName;
            }
            else
            {
                propName = FieldMapper.TranslateToFieldName(propName);
            }

            return propName;
        }
        
        internal static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }
            return null;
        }
    }
}
