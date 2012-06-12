namespace SharepointCommon.Common
{
    using System;

    public static class Assert
    {
        public static void That(bool condition)
        {
            if (condition == false)
                throw new Exception("condition is false");
        }

        public static void NotNull(object expression)
        {
            if (expression == null)
                throw new Exception("expression is null");
        }

        public static void Inconsistent()
        {
            throw new Exception("Incorrect situation");
        }
    }
}