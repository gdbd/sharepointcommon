using System;
using System.Threading;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.ER.Entities
{
    public class AddedItemAsync : CustomItem
    {
        public static AddedItemAsync Received;

        public static ManualResetEvent ManualResetEvent = new ManualResetEvent(false);

        public static Exception Exception;

        [NotMapped]
        public static bool IsAddCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}