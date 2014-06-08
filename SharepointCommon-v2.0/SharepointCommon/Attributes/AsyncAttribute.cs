using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharepointCommon.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AsyncAttribute : Attribute
    {
        public AsyncAttribute()
        {
            IsAsync = true;
        }

        public AsyncAttribute(bool isAsync)
        {
            IsAsync = isAsync;
        }

        public bool IsAsync { get; private set; }
    }
}
