using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.SharePoint;
using SharepointCommon.Common;

namespace SharepointCommon.Events
{
    public class ListItemEventReceiver : SPItemEventReceiver
    {
        #region overriden receiver methods

        public override void ItemAdding(SPItemEventProperties properties)
        {
            EventFiringEnabled = false;
            InvokeIngReceiver(properties, SPEventReceiverType.ItemAdding, "ItemAdding");
            EventFiringEnabled = true;
        }

        public override void ItemAdded(SPItemEventProperties properties)
        {
            EventFiringEnabled = false;
            InvokeEdReceiver(properties, SPEventReceiverType.ItemAdded, "ItemAdded");
            EventFiringEnabled = true;
        }

        public override void ItemUpdating(SPItemEventProperties properties)
        {
            EventFiringEnabled = false;
            InvokeIngReceiver(properties, SPEventReceiverType.ItemUpdating, "ItemUpdating");
            EventFiringEnabled = true;
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            EventFiringEnabled = false;
            InvokeEdReceiver(properties, SPEventReceiverType.ItemUpdated, "ItemUpdated");
            EventFiringEnabled = true;
        }

        public override void ItemDeleting(SPItemEventProperties properties)
        {
            EventFiringEnabled = false;
            InvokeIngReceiver(properties, SPEventReceiverType.ItemDeleting, "ItemDeleting");
            EventFiringEnabled = true;
        }

        public override void ItemDeleted(SPItemEventProperties properties)
        {
            EventFiringEnabled = false;
            InvokeEdReceiver(properties, SPEventReceiverType.ItemDeleted, "ItemDeleted");
            EventFiringEnabled = true;
        }

        #endregion

        private EventReceiverProperties GetEventReceiverType(SPItemEventProperties properties, SPEventReceiverType receiverType)
        {
            var er = properties.List.EventReceivers.Cast<SPEventReceiverDefinition>()
                .FirstOrDefault(e => e.HostId == properties.ListId && e.Type == receiverType);

            Assert.NotNull(er);
            
            return new EventReceiverProperties
            {
                EventReceiverType = Type.GetType(er.Data),
            };
        }

        //Invoke Added/Updated/Deleted receivers
        private void InvokeEdReceiver(SPItemEventProperties properties, SPEventReceiverType eventReceiverType, string methodName)
        {
            var receiverProps = GetEventReceiverType(properties, eventReceiverType);
            var receiver = Activator.CreateInstance(receiverProps.EventReceiverType);
            var receiverMethod = receiverProps.EventReceiverType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            var receiverParam = receiverMethod.GetParameters().First();
            switch (eventReceiverType)
            {
                case SPEventReceiverType.ItemDeleted:
                    receiverMethod.Invoke(receiver, new[] { (object)properties.ListItemId });
                    break;
                default:

#warning remove sleep!
                    Thread.Sleep(1000);

                    var entityType = EntityMapper.ToEntity(receiverParam.ParameterType, properties.ListItem);
                    receiverMethod.Invoke(receiver, new[] { entityType });
                    break;
            }
        }
        
        //Invoke Adding/Updating/Deleting receivers
        private void InvokeIngReceiver(SPItemEventProperties properties, SPEventReceiverType eventReceiverType, string methodName)
        {
            var afterProperties = new Hashtable();
            foreach (DictionaryEntry afterProperty in properties.AfterProperties)
            {
                afterProperties.Add(afterProperty.Key, afterProperty.Value);
            }

            var receiverProps = GetEventReceiverType(properties, eventReceiverType);
            var receiver = Activator.CreateInstance(receiverProps.EventReceiverType);
            var method = receiverProps.EventReceiverType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            var receiverParam = method.GetParameters().First();
        
            switch (eventReceiverType)
            {
                case SPEventReceiverType.ItemAdding:
                    var entity = EntityMapper.ToEntity(receiverParam.ParameterType, afterProperties, properties.List);
                    method.Invoke(receiver, new[] { entity });
                    break;

                case SPEventReceiverType.ItemUpdating:
                    entity = EntityMapper.ToEntity(receiverParam.ParameterType, properties.ListItem, false);
                    var changedItem = EntityMapper.ToEntity(receiverParam.ParameterType, afterProperties, properties.List);
                    
                    method.Invoke(receiver, new[] { entity, changedItem });
                    break;

                case SPEventReceiverType.ItemDeleting:
                    entity = EntityMapper.ToEntity(receiverParam.ParameterType, properties.ListItem, false);
                    method.Invoke(receiver, new[] { entity });
                    break;
            }
        }
    }
}