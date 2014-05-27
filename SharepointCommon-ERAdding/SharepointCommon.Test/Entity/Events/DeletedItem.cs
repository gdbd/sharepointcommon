using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Entity.Events
{
    public class DeletedItem : Item
    {
        [NotMapped]
        public static int DeletedId { get; set; }

        [NotMapped]
        public static bool IsDeleteCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}
