using System.Reflection;
using SharepointCommon.Attributes;
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
            //var list = new QueryList<Item>()
            return queryWeb.GetById<Item>(listId);
        }
    }
}
