using System;
using System.Threading;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.ER.Entities
{
    public class UpdatedItemAsync : CustomItem
    {
        public static UpdatedItemAsync Received;

        public static ManualResetEvent ManualResetEvent = new ManualResetEvent(false);

        public static Exception Exception;

        [NotMapped]
        public static bool IsUpdateCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}
