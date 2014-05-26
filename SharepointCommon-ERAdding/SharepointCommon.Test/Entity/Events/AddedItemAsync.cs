using System.Threading;
using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Entity.Events
{
    public class AddedItemAsync : Item
    {
        public static ManualResetEvent ManualResetEvent = new ManualResetEvent(false);

        [NotMapped]
        public static bool IsAddCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}