namespace XRMFramework.Util.Validation
{
    public static class Int32Validation
    {
        public static int IsGreaterThan(this int val, int comparedTo, string errorMessage = null)
         => ValueTypeComparisons<int>.IsGreaterThan(val, comparedTo, errorMessage);
    }
}