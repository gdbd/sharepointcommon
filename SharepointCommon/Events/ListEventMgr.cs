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
            var registeredEvents = GetRegisteredReceivers<TEventReceiver>();

            foreach (var eventReceiverInfo in registeredEvents)
            {
                if(!CheckRegistrationValid(eventReceiverInfo))
                    throw new SharepointCommonException("Asynchronous execution of before events is invalid.");

                RegisterEventReceiver(receiverType.AssemblyQualifiedName, list, eventReceiverInfo);
            }
        }

        private static bool CheckRegistrationValid(EventReceiverInfo erInfo)
        {
            if(erInfo.Synchronization == SPEventReceiverSynchronization.Asynchronous 
                && (erInfo.Type == SPEventReceiverType.ItemAdding
                || erInfo.Type == SPEventReceiverType.ItemUpdating
                || erInfo.Type == SPEventReceiverType.ItemDeleting))
                return false;
            
            return true;
        }

        internal static void RemoveEventReceiver<TEventReceiver>(SPList list)
        {
            var receiverType = typeof (TEventReceiver);
            var registeredEvents = GetRegisteredReceivers<TEventReceiver>();

            foreach (var eventReceiverInfo in registeredEvents)
            {
                UnRegisterEventReceiver(receiverType.AssemblyQualifiedName, list, eventReceiverInfo);
            }
        }
        
        private static IEnumerable<EventReceiverInfo> GetRegisteredReceivers<TEventReceiver>()
        {
            var receiverType = typeof (TEventReceiver);

            var methods = new List<MethodInfo>();
            methods.Add(receiverType.GetMethod("ItemAdded", BindingFlags.Instance | BindingFlags.Public));
            methods.Add(receiverType.GetMethod("ItemAdding", BindingFlags.Instance | BindingFlags.Public));
            methods.Add(receiverType.GetMethod("ItemUpdating", BindingFlags.Instance | BindingFlags.Public));
            methods.Add(receiverType.GetMethod("ItemUpdated", BindingFlags.Instance | BindingFlags.Public));
            methods.Add(receiverType.GetMethod("ItemDeleting", BindingFlags.Instance | BindingFlags.Public));
            methods.Add(receiverType.GetMethod("ItemDeleted", BindingFlags.Instance | BindingFlags.Public));

            foreach (var method in methods)
            {
                if (!IsMethodOverriden(method)) continue;
                yield return GetEventReceiverInfo(method);
            }
        }

        private static EventReceiverInfo GetEventReceiverInfo(MethodInfo method)
        {
            var ret = new EventReceiverInfo();
            ret.Synchronization = SPEventReceiverSynchronization.Default;
            ret.Sequence = 10000;

            SPEventReceiverType res;
            if (Enum.TryParse(method.Name, out res))
            {
                ret.Type = res;
            }
            else
            {
                throw new SharepointCommonException(string.Format("Cannot determine event type: {0}", method.Name));
            }
         

            var async = (AsyncAttribute) Attribute.GetCustomAttribute(method, typeof (AsyncAttribute));

            if (async != null)
            {
                ret.Synchronization = async.IsAsync
                    ? SPEventReceiverSynchronization.Asynchronous
                    : SPEventReceiverSynchronization.Synchronous;
            }

            var sequence = (SequenceAttribute) Attribute.GetCustomAttribute(method, typeof (SequenceAttribute));

            if (sequence != null)
            {
                ret.Sequence = sequence.Sequence;
            }

            return ret;
        }

        private static bool IsMethodOverriden(MethodInfo method)
        {
            // todo: get parent type recursive!
            return method.DeclaringType != null && method.DeclaringType.Name != "ListEventReceiver`1";
        }
        
        private static void RegisterEventReceiver(string handlerClassName, SPList list, EventReceiverInfo eventReceiverInfo)
        {
            // save link repository<=>list by set eventreceiver name to Repository.Type.AssemblyQualifiedName
            // add SharepointCommon.Events.ListItemEventReceiver to SPList.EventReceivers
           
            if (GetReceiver(handlerClassName, list, eventReceiverInfo) != null) return;
            
            var er = list.EventReceivers.Add();
            er.Name = string.Format("SharepointCommon [{0}] handler for [{1}]",
                Enum.GetName(typeof (SPEventReceiverType), eventReceiverInfo.Type), list.RootFolder.ServerRelativeUrl);
            er.Assembly = Assembly.GetExecutingAssembly().FullName;
            er.Class = "SharepointCommon.Events.ListItemEventReceiver";
            er.Type = eventReceiverInfo.Type;
            er.Data = handlerClassName;
            er.SequenceNumber = eventReceiverInfo.Sequence;
            er.Synchronization = eventReceiverInfo.Synchronization;
            er.Update();
        }

        private static void UnRegisterEventReceiver(string handlerClassName, SPList list, EventReceiverInfo eventReceiverInfo)
        {
            var spEventReceiverDefinition = GetReceiver(handlerClassName, list, eventReceiverInfo);
            if (spEventReceiverDefinition == null) return;
            spEventReceiverDefinition.Delete();
        }

        private static SPEventReceiverDefinition GetReceiver(string handlerClassName, SPList list, EventReceiverInfo eventReceiverInfo)
        {
            return list.EventReceivers.Cast<SPEventReceiverDefinition>().FirstOrDefault(e =>
                e.Assembly == Assembly.GetExecutingAssembly().FullName &&
                e.Class == "SharepointCommon.Events.ListItemEventReceiver" &&
                e.Type == eventReceiverInfo.Type &&
                e.Data == handlerClassName &&

                //todo: check this
                e.Synchronization == eventReceiverInfo.Synchronization &&
                e.SequenceNumber == eventReceiverInfo.Sequence);

        }
    }
}
