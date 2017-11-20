using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.SharePoint;
using SharepointCommon.Attributes;
using SharepointCommon.Common;

namespace SharepointCommon.Events
{
    /// <summary>
    /// For internal use only
    /// </summary>
    public class ListItemEventReceiver : SPItemEventReceiver
    {
        #region overriden receiver methods

        public override void ItemAdding(SPItemEventProperties properties)
        {
            InvokeIngReceiver(properties, SPEventReceiverType.ItemAdding, "ItemAdding");
        }

        public override void ItemAdded(SPItemEventProperties properties)
        {
            InvokeEdReceiver(properties, SPEventReceiverType.ItemAdded, "ItemAdded");
        }

        public override void ItemUpdating(SPItemEventProperties properties)
        {
            InvokeIngReceiver(properties, SPEventReceiverType.ItemUpdating, "ItemUpdating");
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            InvokeEdReceiver(properties, SPEventReceiverType.ItemUpdated, "ItemUpdated");
        }

        public override void ItemDeleting(SPItemEventProperties properties)
        {
            InvokeIngReceiver(properties, SPEventReceiverType.ItemDeleting, "ItemDeleting");
        }

        public override void ItemDeleted(SPItemEventProperties properties)
        {
            InvokeEdReceiver(properties, SPEventReceiverType.ItemDeleted, "ItemDeleted");
        }

        #endregion

        private EventReceiverProperties GetEventReceiverType(SPItemEventProperties properties, SPEventReceiverType receiverType)
        {
            var er = properties.List.EventReceivers.Cast<SPEventReceiverDefinition>()
                .FirstOrDefault(e => e.HostId == properties.ListId && e.Type == receiverType 
                    && !string.IsNullOrEmpty(e.Data));

            Assert.NotNull(er);


            var eventReceiverType = Type.GetType(er.Data);

            return new EventReceiverProperties
            {
                EventReceiverType = eventReceiverType,
            };
        }

        //Invoke Added/Updated/Deleted receivers
        private void InvokeEdReceiver(SPItemEventProperties properties, SPEventReceiverType eventReceiverType, string methodName)
        {
            try
            {
                var receiverProps = GetEventReceiverType(properties, eventReceiverType);
                var receiver = Activator.CreateInstance(receiverProps.EventReceiverType);
                var method = receiverProps.EventReceiverType.GetMethod(methodName,BindingFlags.Instance | BindingFlags.Public);
                var receiverParam = method.GetParameters().First();

                var eventDisabled = GetEventFiringDisabled(method);

                bool origDisabledValue = false;
                try
                {
                    if (eventDisabled)
                    {
                        origDisabledValue = base.EventFiringEnabled;
                        base.EventFiringEnabled = false;
                    }


                    switch (eventReceiverType)
                    {
                        case SPEventReceiverType.ItemDeleted:
                            method.Invoke(receiver, new[] {(object) properties.ListItemId});
                            break;
                        default:
                            var entityType = EntityMapper.ToEntity(receiverParam.ParameterType, properties.ListItem);
                            method.Invoke(receiver, new[] {entityType});
                            break;
                    }
                }
                finally
                {
                    if (eventDisabled)
                    {
                        base.EventFiringEnabled = origDisabledValue;
                    }
                }

                ProccessCancel(receiver, properties);
            }
            catch (TargetInvocationException tex)
            {
                throw tex.InnerException;
            }
        }
        
        //Invoke Adding/Updating/Deleting receivers
        private void InvokeIngReceiver(SPItemEventProperties properties, SPEventReceiverType eventReceiverType, string methodName)
        {
            try
            {
                var afterProperties = new Hashtable();
                foreach (DictionaryEntry afterProperty in properties.AfterProperties)
                {
                    afterProperties.Add(afterProperty.Key, afterProperty.Value);
                }

                var receiverProps = GetEventReceiverType(properties, eventReceiverType);
                var receiver = Activator.CreateInstance(receiverProps.EventReceiverType);
                var method = receiverProps.EventReceiverType.GetMethod(methodName,
                    BindingFlags.Instance | BindingFlags.Public);
                var receiverParam = method.GetParameters().First();

                var eventDisabled = GetEventFiringDisabled(method);

                bool origDisabledValue = false;
                try
                {
                    if (eventDisabled)
                    {
                        origDisabledValue = base.EventFiringEnabled;
                        base.EventFiringEnabled = false;
                    }

                    switch (eventReceiverType)
                    {
                        case SPEventReceiverType.ItemAdding:
                            var entity = EntityMapper.ToEntity(receiverParam.ParameterType, afterProperties,
                                properties.List);
                            method.Invoke(receiver, new[] {entity});
                            break;

                        case SPEventReceiverType.ItemUpdating:
                            entity = EntityMapper.ToEntity(receiverParam.ParameterType, properties.ListItem, false);
                            var changedItem = EntityMapper.ToEntity(receiverParam.ParameterType, afterProperties,
                                properties.List);

                            method.Invoke(receiver, new[] {entity, changedItem});
                            break;

                        case SPEventReceiverType.ItemDeleting:
                            entity = EntityMapper.ToEntity(receiverParam.ParameterType, properties.ListItem, false);
                            method.Invoke(receiver, new[] {entity});
                            break;
                    }
                }
                finally
                {
                    if (eventDisabled)
                    {
                        base.EventFiringEnabled = origDisabledValue;
                    }
                }

                ProccessCancel(receiver, properties);
            }
            catch (TargetInvocationException tex)
            {
                throw tex.InnerException;
            }
        }

        private bool GetEventFiringDisabled(MethodInfo method)
        {
            var attr = (DisableEventFiringAttribute)Attribute.GetCustomAttribute(method, typeof (DisableEventFiringAttribute));
            return attr != null;
        }

        private void ProccessCancel(object receiver, SPItemEventProperties properties)
        {
            var cancelledField = receiver.GetType().GetField("Cancelled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var messageField = receiver.GetType().GetField("Message", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var cancelled = (bool)cancelledField.GetValue(receiver);

            if (cancelled)
            {
                var message = (string)messageField.GetValue(receiver);

                if (string.IsNullOrEmpty(message))
                {
                    properties.Status = SPEventReceiverStatus.CancelNoError;
                }
                else
                {
                    properties.ErrorMessage = message;
                    properties.Status = SPEventReceiverStatus.CancelWithError;
                }
            }
        }
    }
}