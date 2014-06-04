using System;
using System.Threading;
using SharepointCommon.Attributes;

namespace SharepointCommon.Test.ER.Entities
{
    public class DeletedItemAsync : Item
    {
        public static Exception Exception;
        public static ManualResetEvent ManualResetEvent = new ManualResetEvent(false);

        [NotMapped]
        public static int DeletedId { get; set; }

        [NotMapped]
        public static bool IsDeleteCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}