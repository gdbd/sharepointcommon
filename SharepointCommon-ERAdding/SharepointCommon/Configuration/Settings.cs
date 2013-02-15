using System.Collections.Generic;

namespace SharepointCommon.Configuration
{
    public class Settings
    {
        public IEnumerable<EventReceiver> EventReceivers { get; set; }

        public class EventReceiver
        {
            public string Assembly { get; set; }
            public string ReceicerType { get; set; }
            public string EventType { get; set; }
            public int Sequence { get; set; }
            public bool Async { get; set; }
        }
    }
}