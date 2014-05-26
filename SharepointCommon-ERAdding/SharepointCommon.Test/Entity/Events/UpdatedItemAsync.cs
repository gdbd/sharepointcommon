using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Entity.Events
{
    public class UpdatedItemAsync : Item
    {
        public static ManualResetEvent ManualResetEvent = new ManualResetEvent(false);

        public static Exception Exception;

        [NotMapped]
        public static bool IsUpdateCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}
