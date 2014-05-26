using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Microsoft.SharePoint;
using SharepointCommon.Common;

namespace SharepointCommon.Events
{
    public class ListItemEventReceiver : SPItemEventReceiver
    {
        public override void ItemAdding(SPItemEventProperties properties)
        {
            EventFiringEnabled = false;
            InvokeIngReceiver(properties,SPEventReceiverType.ItemAdding, "ItemAdding");
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
            base.ItemUpdating(properties);
            InvokeIngReceiver(properties, SPEventReceiverType.ItemUpdating, "ItemUpdating");
            EventFiringEnabled = true;
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            EventFiringEnabled = false;
            base.ItemUpdated(properties);
            InvokeEdReceiver(properties,SPEventReceiverType.ItemUpdated, "ItemUpdated");
            EventFiringEnabled = true;
        }

        public override void ItemDeleting(SPItemEventProperties properties)
        {
            EventFiringEnabled = false;
            base.ItemDeleting(properties);
            InvokeIngReceiver(properties, SPEventReceiverType.ItemDeleting, "ItemDeleting");
            EventFiringEnabled = true;
        }

        public override void ItemDeleted(SPItemEventProperties properties)
        {
            EventFiringEnabled = false;
            base.ItemDeleted(properties);
            InvokeEdReceiver(properties, SPEventReceiverType.ItemDeleted, "ItemDeleted");
            EventFiringEnabled = true;
        }

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
                    var entityType = EntityMapper.ToEntity(receiverParam.ParameterType, properties.ListItem);
                    receiverMethod.Invoke(receiver, new[] { entityType });
                    break;
            }

            
        }
        
        //Invoke Adding/Updating/Deleting receivers
        private void InvokeIngReceiver(SPItemEventProperties properties, SPEventReceiverType eventReceiverType, string methodName)
        {
            var hashTable = new Hashtable();
            foreach (DictionaryEntry afterProperty in properties.AfterProperties)
            {
                hashTable.Add(afterProperty.Key, afterProperty.Value);
            }
            var receiverProps = GetEventReceiverType(properties, eventReceiverType);
            var receiver = Activator.CreateInstance(receiverProps.EventReceiverType);
            var method = receiverProps.EventReceiverType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            var receiverParam = method.GetParameters().First();
            object entity;
            switch (eventReceiverType)
            {
                case SPEventReceiverType.ItemAdding:
                    entity = EntityMapper.ToEntity(receiverParam.ParameterType, hashTable, properties.List);
                    method.Invoke(receiver, new[] { entity });
                    break;
                case SPEventReceiverType.ItemUpdating:
                    entity = EntityMapper.ToEntity(receiverParam.ParameterType, properties.ListItem);
                    var changedItem = EntityMapper.ToEntity(receiverParam.ParameterType, hashTable, properties.List);
                    method.Invoke(receiver, new[] { entity, changedItem });
                    break;
                case SPEventReceiverType.ItemDeleting:
                    entity = EntityMapper.ToEntity(receiverParam.ParameterType, properties.ListItem);
                    method.Invoke(receiver, new[] { entity });
                    break;

            }

           
            foreach (DictionaryEntry property in properties.AfterProperties)
            {
                if (hashTable.ContainsKey(property.Key) && hashTable[property.Key] == property.Value)
                {
                    hashTable.Remove(property.Key);
                }
            }
            foreach (DictionaryEntry entry in hashTable)
            {
                properties.AfterProperties.ChangedProperties.Add(entry.Key, entry.Value);
            }
        }
    }
}