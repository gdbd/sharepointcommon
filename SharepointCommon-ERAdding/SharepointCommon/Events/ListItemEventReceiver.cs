using System;
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
            base.ItemAdding(properties);
        }

        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);

            var receiverProps = GetEventReceiverType(properties);
            var receiver = Activator.CreateInstance(receiverProps.EventReceiverType);

            var added = receiverProps.EventReceiverType.GetMethod("ItemAdded", BindingFlags.Instance | BindingFlags.NonPublic);
            added.Invoke(receiver, new object[] { null });
        }

        public override void ItemUpdating(SPItemEventProperties properties)
        {
            base.ItemUpdating(properties);
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);
        }

        public override void ItemDeleting(SPItemEventProperties properties)
        {
            base.ItemDeleting(properties);
        }

        public override void ItemDeleted(SPItemEventProperties properties)
        {
            base.ItemDeleted(properties);
        }

        private EventReceiverProperties GetEventReceiverType(SPItemEventProperties properties)
        {
            var er = properties.List.EventReceivers.Cast<SPEventReceiverDefinition>()
                .FirstOrDefault(e => e.HostId == properties.ListId && e.Type == SPEventReceiverType.ItemAdded);

            Assert.NotNull(er);
            
            return new EventReceiverProperties
            {
                EventReceiverType = Type.GetType(er.Data),
            };
        }
    }
}