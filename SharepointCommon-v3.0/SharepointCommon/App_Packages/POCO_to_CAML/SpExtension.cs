using System;

namespace CodeToCaml
{
    public static class SpExtension
    {
        private const string NotSupportedErrorMessage =
            "This extension can only be called from within a CAML expression.";

        public static bool Includes(this bool type, bool value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        public static bool Includes(this decimal type, decimal value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        public static bool Includes(this double type, double value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        public static bool Includes(this float type, float value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        public static bool Includes(this int type, int value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        public static bool Includes(this string type, string value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        public static bool NotIncludes(this bool type, bool value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        public static bool NotIncludes(this decimal type, decimal value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        public static bool NotIncludes(this double type, double value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        public static bool NotIncludes(this float type, float value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        public static bool NotIncludes(this int type, int value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        public static bool NotIncludes(this string type, string value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }
    }
}
