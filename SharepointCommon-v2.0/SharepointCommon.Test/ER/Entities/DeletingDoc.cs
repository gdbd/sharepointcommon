using System;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.ER.Entities
{
    public class DeletingDoc : CustomDocument
    {
        public static DeletingDoc Received;
        public static Exception Exception;

        [NotMapped]
        public static bool IsDeleteCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}
