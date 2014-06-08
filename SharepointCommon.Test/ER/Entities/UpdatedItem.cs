using System;
using System.Linq;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.ER.Entities
{
    public class UpdatedItem : CustomItem
    {
        public static UpdatedItem Recieved;
        public static Exception Exception;
        
        [NotMapped]
        public static bool IsUpdateCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}
