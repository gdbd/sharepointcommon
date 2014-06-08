using System;
using Microsoft.SharePoint;

namespace SharepointCommon.Events
{
    internal class EventReceiverInfo
    {
        public SPEventReceiverType Type;
        public SPEventReceiverSynchronization Synchronization;
        public int Sequence;
    }
}