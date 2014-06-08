using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharepointCommon.Attributes;

namespace SharepointCommon.Test.Entity
{
    public class CascadeLookup : Item
    {
        [Field(LookupList = "Cascade_Lookup_Test")]
        public virtual CascadeLookup Parent { get; set; }
    }
}
