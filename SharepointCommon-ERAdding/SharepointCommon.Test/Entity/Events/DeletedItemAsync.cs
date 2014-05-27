using System.Threading;
using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Entity.Events
{
    public class DeletedItemAsync : Item
    {
        public static ManualResetEvent ManualResetEvent = new ManualResetEvent(false);

        [NotMapped]
        public static int DeletedId { get; set; }

        [NotMapped]
        public static bool IsDeleteCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}