using System;
using SharepointCommon.Attributes;

namespace SharepointCommon.Test.ER.Entities
{
    public class AddRemoveTest : Item
    {
        public static Exception Exception;

        [NotMapped]
        public static bool IsAddCalled { get; set; }

        [NotMapped]
        public static bool IsUpdateCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}
