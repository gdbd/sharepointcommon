using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharepointCommon.Attributes
{
    /// <summary>
    /// Mark <see cref="ListEventReceiver{T}"/> methods to
    /// set event receiver is synchronous or asynchronous
    /// </summary>
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
