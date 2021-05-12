using System;

namespace XRMFramework.Util.Validation
{
    public static class ObjectValidations
    {
        public static TType IsNotNull<TType>(this TType obj, string errorMessage = null) where TType : class
            => obj ?? throw new NullReferenceException(errorMessage ?? $"Argument of type {typeof(TType)} cannot be null");
    }
}