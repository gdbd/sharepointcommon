using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Events
{
    public class EventTestItem : Item
    {
        [NotMapped]
        public static bool IsAddCalled { get; set; }

        [NotMapped]
        public static bool IsUpdateCalled { get; set; }
    }
}
