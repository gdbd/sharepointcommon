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
        internal static void RegisterEventReceivers<TEventReceiver>(SPList list)
        {
            var receiverType = typeof(TEventReceiver);
            var adding = receiverType.GetMethod("ItemAdding", BindingFlags.Instance | BindingFlags.NonPublic);
            var added = receiverType.GetMethod("ItemAdded", BindingFlags.Instance | BindingFlags.NonPublic);
            var updating = receiverType.GetMethod("ItemUpdating", BindingFlags.Instance | BindingFlags.NonPublic);
            var updated = receiverType.GetMethod("ItemUpdated", BindingFlags.Instance | BindingFlags.NonPublic);
            var deleting = receiverType.GetMethod("ItemDeleting", BindingFlags.Instance | BindingFlags.NonPublic);
            var deleted = receiverType.GetMethod("ItemDeleted", BindingFlags.Instance | BindingFlags.NonPublic);

            ProccessMethod(list, adding, receiverType, SPEventReceiverType.ItemAdding);
            ProccessMethod(list, added, receiverType, SPEventReceiverType.ItemAdded);
            ProccessMethod(list, updating, receiverType, SPEventReceiverType.ItemUpdating);
            ProccessMethod(list, updated, receiverType, SPEventReceiverType.ItemUpdated);
            ProccessMethod(list, deleting, receiverType, SPEventReceiverType.ItemDeleting);
            ProccessMethod(list, deleted, receiverType, SPEventReceiverType.ItemDeleted);
        }

        internal static void RemoveEventReceiver<TEventReceiver>(SPList list)
        {
            
        }

        private static void ProccessMethod(SPList list, MethodInfo method, Type handlerType,
            SPEventReceiverType eventType)
        {
            if (IsMethodOverriden(method))
            {
                var async = (AsyncAttribute) Attribute.GetCustomAttribute(method, typeof (AsyncAttribute));
                var sequence = (SequenceAttribute) Attribute.GetCustomAttribute(method, typeof (SequenceAttribute));
                var synchronization = SPEventReceiverSynchronization.Default;
                if (async != null && async.IsAsync)
                    synchronization = SPEventReceiverSynchronization.Asynchronous;
                else if (async != null && !async.IsAsync)
                    synchronization = SPEventReceiverSynchronization.Synchronous;

                // ReSharper disable once PossibleNullReferenceException
                RegisterEventReceiver(handlerType.AssemblyQualifiedName,
                    list, eventType,
                    synchronization,
                    sequence == null ? 10000 : sequence.Sequence);
            }
        }

        private static void RegisterEventReceiver(string handlerClassName, SPList list, SPEventReceiverType type, SPEventReceiverSynchronization async, int sequence)
        {
            // save link repository<=>list by set eventreceiver name to Repository.Type.AssemblyQualifiedName
            // add SharepointCommon.Events.ListItemEventReceiver to SPList.EventReceivers
           
            if (list.EventReceivers.Cast<SPEventReceiverDefinition>().Any(e =>
                e.Assembly == Assembly.GetExecutingAssembly().FullName &&
                e.Class == "SharepointCommon.Events.ListItemEventReceiver" &&
                e.Type == type &&
                e.Data == handlerClassName &&
              //  e.Synchronization == synchronization &&
                e.SequenceNumber == sequence)) return;
            
            var er = list.EventReceivers.Add();
            er.Name = string.Format("SharepointCommon [{0}] handler for [{1}]",
                Enum.GetName(typeof (SPEventReceiverType), type), list.RootFolder.ServerRelativeUrl);
            er.Assembly = Assembly.GetExecutingAssembly().FullName;
            er.Class = "SharepointCommon.Events.ListItemEventReceiver";
            er.Type = type;
            er.Data = handlerClassName;
            er.SequenceNumber = sequence;
            er.Synchronization = async;
            er.Update();
        }

        private static bool IsMethodOverriden(MethodInfo method)
        {
            return method.DeclaringType != null && method.DeclaringType.Name != "ListEventReceiver`1";
        }
    }
}
