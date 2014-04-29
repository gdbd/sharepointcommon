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

            // read EventReceivers by HostId == properties.ListId
            // construct repository by type of eventreceiver.Name and call event method

            var er = properties.List.EventReceivers.Cast<SPEventReceiverDefinition>()
                .FirstOrDefault(e => e.HostId == properties.ListId && e.Type == SPEventReceiverType.ItemAdded);

            Assert.NotNull(er);

            var repositoryType = Type.GetType(er.Data);


            // todo: get repository instance from lists cache instead
            var repository = Activator.CreateInstance(repositoryType);

            var listProp = repositoryType.GetProperty("List");
            var webProp = repositoryType.GetProperty("ParentWeb");
            listProp.SetValue(repository, properties.List, null);
            webProp.SetValue(repository, WebFactory.Open(properties.Web.Url), null);

            var added = repositoryType.GetMethod("ItemAdded", BindingFlags.Instance | BindingFlags.NonPublic);
            added.Invoke(repository, new object[] { null });
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
    }
}