using System;
using SharepointCommon.Attributes;

namespace SharepointCommon.Test.ER.Entities
{
    public class DeletedItem : Item
    {
        public static Exception Exception;

        [NotMapped]
        public static int DeletedId { get; set; }

        [NotMapped]
        public static bool IsDeleteCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}
