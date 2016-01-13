using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharepointCommon.Test
{
    public class Helper
    {
        public static IEnumerable<T> GetInterceptor<T>(object proxy) where T : Castle.DynamicProxy.IInterceptor
        {
            var fieldIntrCeptors = proxy.GetType().GetField("__interceptors");
            var interceptors = (Castle.DynamicProxy.IInterceptor[])fieldIntrCeptors.GetValue(proxy);
            return interceptors.OfType<T>();
        }
    }
}
