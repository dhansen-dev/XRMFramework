namespace XRMFramework.Util.Validation
{
    public static class DoubleValidations
    {
        public static double IsGreaterThan(this double value, double compareTo, string errorMessage = null)
            => ValueTypeComparisons<double>.IsGreaterThan(value, compareTo, errorMessage);
    }
}