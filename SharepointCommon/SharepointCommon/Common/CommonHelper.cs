namespace SharepointCommon.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.SharePoint;

    using SharepointCommon.Exceptions;

    internal sealed class CommonHelper
    {
        internal static SPUser GetUser(SPListItem item, string fieldStaticName)
        {
            var userField = (SPFieldUser)item.Fields.TryGetFieldByStaticName(fieldStaticName);
            if (userField == null) throw new SharepointCommonException(string.Format("Field {0} not exist", fieldStaticName));

            if (item[fieldStaticName] == null) return null;

            var fieldValue = (SPFieldUserValue)userField.GetFieldValue(item[fieldStaticName].ToString());
            return fieldValue.User;
        }

        internal static SPFieldUserValueCollection GetUsers(SPListItem item, string fieldStaticName)
        {
            var userField = (SPFieldUser)item.Fields.TryGetFieldByStaticName(fieldStaticName);
            if (userField == null) throw new SharepointCommonException(string.Format("Field {0} not exist", fieldStaticName));

            if (item[fieldStaticName] == null) return null;

            return (SPFieldUserValueCollection)userField.GetFieldValue(item[fieldStaticName].ToString());
        }

        /// <summary>Determines whether a type, like IList&lt;int&gt;, implements an open generic interface, like
        /// IEnumerable&lt;&gt;. Note that this only checks against *interfaces*.</summary>
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
    }
}
