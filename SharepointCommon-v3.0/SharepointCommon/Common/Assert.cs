using System.Reflection;
using System;
using Microsoft.SharePoint;

namespace SharepointCommon.Common
{
    internal static class Assert
    {
        internal static void That(bool condition)
        {
            if (condition == false)
                throw new Exception("condition is false");
        }

        internal static void NotNull(object expression)
        {
            if (expression == null)
                throw new Exception("expression is null");
        }

        internal static void Inconsistent()
        {
            throw new Exception("Incorrect situation");
        }

        internal static void CurrentContextAvailable()
        {
            if (SPContext.Current == null) throw new SharepointCommonException("SPContext.Current not available");
        }

        internal static void IsPropertyVirtual(PropertyInfo prop)
        {
            var methodGet = prop.GetGetMethod();
            var methodSet = prop.GetSetMethod();

            bool isVirtual = methodGet != null && methodGet.IsVirtual;

            isVirtual = (methodSet != null && methodSet.IsVirtual) || isVirtual;

            if (isVirtual == false) throw new SharepointCommonException(string.Format("Property {0} must be virtual to work correctly.", prop.Name));
        }
    }
}