using System;

namespace XRMFramework.Util.Validation
{
    public static class ValidationHelper
    {
        public static void CreateExceptionOfType<TException>(string errorMessage) where TException : Exception, new()
            => throw (TException)Activator.CreateInstance(typeof(TException), errorMessage);
    }
}