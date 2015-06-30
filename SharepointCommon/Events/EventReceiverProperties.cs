using System;

namespace SharepointCommon.Events
{
    internal class EventReceiverProperties
    {
        public Type EventReceiverType { get; set; }
        public bool IsAsync { get; set; }
        public bool Sequence { get; set; }
    }
}