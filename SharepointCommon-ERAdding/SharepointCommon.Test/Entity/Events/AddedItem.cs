using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Entity.Events
{
    public class AddedItem : Item
    {
        [NotMapped]
        public static bool IsAddCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}
