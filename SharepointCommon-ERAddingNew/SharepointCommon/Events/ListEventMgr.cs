using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.SharePoint;
using SharepointCommon.Attributes;
using SharepointCommon.Common;

namespace SharepointCommon.Events
{
    internal class ListEventMgr
    {
        internal static void RegisterEventReceivers(Type repositoryType, SPList list)
        {
            var adding = repositoryType.GetMethod("ItemAdding", BindingFlags.Instance | BindingFlags.NonPublic);
            var added = repositoryType.GetMethod("ItemAdded", BindingFlags.Instance | BindingFlags.NonPublic);
            var updating = repositoryType.GetMethod("ItemUpdating", BindingFlags.Instance | BindingFlags.NonPublic);
            var updated = repositoryType.GetMethod("ItemUpdated", BindingFlags.Instance | BindingFlags.NonPublic);
            var deleting = repositoryType.GetMethod("ItemDeleting", BindingFlags.Instance | BindingFlags.NonPublic);
            var deleted = repositoryType.GetMethod("ItemDeleted", BindingFlags.Instance | BindingFlags.NonPublic);

            ProccessMethod(list, adding, repositoryType, SPEventReceiverType.ItemAdding);
            ProccessMethod(list, added, repositoryType, SPEventReceiverType.ItemAdded);
            ProccessMethod(list, updating, repositoryType, SPEventReceiverType.ItemUpdating);
            ProccessMethod(list, updated, repositoryType, SPEventReceiverType.ItemUpdated);
            ProccessMethod(list, deleting, repositoryType, SPEventReceiverType.ItemDeleting);
            ProccessMethod(list, deleted, repositoryType, SPEventReceiverType.ItemDeleted);
        }

        private static void ProccessMethod(SPList list, MethodInfo method, Type repositoryType, SPEventReceiverType eventType)
        {
            if (IsMethodOverriden(method))
            {
                var async = (AsyncAttribute)Attribute.GetCustomAttribute(method, typeof(AsyncAttribute));
                var sequence = (SequenceAttribute)Attribute.GetCustomAttribute(method, typeof(SequenceAttribute));

                Assert.NotNull(method.DeclaringType);

                // ReSharper disable once PossibleNullReferenceException
                RegisterEventReceiver(method.DeclaringType.AssemblyQualifiedName,
                    list, eventType, 
                    async != null && async.IsAsync,
                    sequence == null ? 10000 : sequence.Sequence);
            }
        }

        private static void RegisterEventReceiver(string repositoryClassName, SPList list, SPEventReceiverType type, bool async, int sequence)
        {
            // save link repository<=>list by set eventreceiver name to Repository.Type.AssemblyQualifiedName
            // add SharepointCommon.Events.ListItemEventReceiver to SPList.EventReceivers
            var synchronization = async
                ? SPEventReceiverSynchronization.Asynchronous
                : SPEventReceiverSynchronization.Default;

            if (list.EventReceivers.Cast<SPEventReceiverDefinition>().Any(e =>
                e.Assembly == Assembly.GetExecutingAssembly().FullName &&
                e.Class == "SharepointCommon.Events.ListItemEventReceiver" &&
                e.Type == type &&
                e.Data == repositoryClassName &&
              //  e.Synchronization == synchronization &&
                e.SequenceNumber == sequence)) return;
            
            var er = list.EventReceivers.Add();
            er.Name = string.Format("SharepointCommon [{0}] handler for [{1}]",
                Enum.GetName(typeof (SPEventReceiverType), type), list.RootFolder.ServerRelativeUrl);
            er.Assembly = Assembly.GetExecutingAssembly().FullName;
            er.Class = "SharepointCommon.Events.ListItemEventReceiver";
            er.Type = type;
            er.Data = repositoryClassName;
            er.SequenceNumber = sequence;
            er.Synchronization = synchronization;
            er.Update();
        }

        private static bool IsMethodOverriden(MethodInfo method)
        {
            return method.DeclaringType != null && method.DeclaringType.Name != "ListBase`1";
        }
    }
}
