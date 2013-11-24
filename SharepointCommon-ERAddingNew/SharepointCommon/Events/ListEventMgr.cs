using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.SharePoint;

namespace SharepointCommon.Events
{
    internal class ListEventMgr
    {
        internal static void RegisterEventReceivers(Type repositoryType)
        {
            var adding = repositoryType.GetMethod("ItemAdding", BindingFlags.Instance | BindingFlags.Public);
            var added = repositoryType.GetMethod("ItemAdded", BindingFlags.Instance | BindingFlags.Public);
            var updating = repositoryType.GetMethod("ItemUpdating", BindingFlags.Instance | BindingFlags.Public);
            var updated = repositoryType.GetMethod("ItemUpdated", BindingFlags.Instance | BindingFlags.Public);
            var deleting = repositoryType.GetMethod("ItemDeleting", BindingFlags.Instance | BindingFlags.Public);
            var deleted = repositoryType.GetMethod("ItemDeleted", BindingFlags.Instance | BindingFlags.Public);

            ProccessMethod(adding, repositoryType, SPEventReceiverType.ItemAdding);
            ProccessMethod(added, repositoryType, SPEventReceiverType.ItemAdded);
            ProccessMethod(updating, repositoryType, SPEventReceiverType.FieldUpdating);
            ProccessMethod(updated, repositoryType, SPEventReceiverType.ItemUpdated);
            ProccessMethod(deleting, repositoryType, SPEventReceiverType.ItemDeleting);
            ProccessMethod(deleted, repositoryType, SPEventReceiverType.ItemDeleted);
        }

        private static void ProccessMethod(MethodInfo method, Type repositoryType, SPEventReceiverType eventType)
        {
            if (IsMethodOverriden(method))
            {
                var list = GetList(repositoryType);
                RegisterEventReceiver(list, eventType);
            }
        }

        private static SPList GetList(Type repositoryType)
        {
            throw new NotImplementedException();
        }

        private static void RegisterEventReceiver(SPList list, SPEventReceiverType type)
        {
            // save link repository<=>list by set eventreceiver name to Repository.Type.FullName
            // add SharepointCommon.Events.ListItemEventReceiver to SPList.EventReceivers
            throw new NotImplementedException();
        }

        private static bool IsMethodOverriden(MethodInfo method)
        {
            return method.DeclaringType != null && method.DeclaringType.Name != "ListBase`1";
        }
    }
}
