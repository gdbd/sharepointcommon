using System;
using System.Threading;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.ER.Entities
{
    public class UpdatedDocAsync : CustomDocument
    {
        public static UpdatedDocAsync Received;

        public static ManualResetEvent ManualResetEvent = new ManualResetEvent(false);

        public static Exception Exception;

        [NotMapped]
        public static bool IsUpdateCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}
