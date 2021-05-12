using System;

namespace XRMFramework.Util.Validation
{
    public static class StringValidations
    {
        public static string IsNotNullOrEmpty(this string str, string errorMessage = null)
            => !string.IsNullOrEmpty(str) ? str : throw new ArgumentException(errorMessage ?? "Argument cannot be null or empty");
    }
}