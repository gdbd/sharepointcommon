using System;
using Microsoft.SharePoint;

namespace SharepointCommon.Common
{
    internal class ListItemEventReceiver : SPItemEventReceiver
    {
        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);
            EventFiringEnabled = false;

            // todo : read configuration, fire registered event receivers

            EventFiringEnabled = true;
        }
    }
}