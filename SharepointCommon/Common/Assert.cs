namespace SharepointCommon.Common
{
    using System;
    using Microsoft.SharePoint;

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
    }
}