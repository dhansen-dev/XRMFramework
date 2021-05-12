using System;

namespace XRMFramework.Util.Validation
{
    public abstract class ValueTypeComparisons<TType> where TType : struct, IComparable, IComparable<TType>,
               IConvertible, IEquatable<TType>, IFormattable
    {
        public static TType IsEqualOrGreaterThan(TType val, TType comparedTo, string errorMessage = null)
        => val.CompareTo(comparedTo) < 0 ? throw new ArgumentException(errorMessage ?? $"Value ({val}) must be equal or greater than {comparedTo}") : val;

        public static TType IsEqualOrLessThan(TType val, TType comparedTo, string errorMessage = null)
        => val.CompareTo(comparedTo) > 0 ? throw new ArgumentException(errorMessage ?? $"Value ({val}) must be equal or less than {comparedTo}") : val;

        public static TType IsEqualTo(TType val, TType comparedTo, string errorMessage = null)
            => val.CompareTo(comparedTo) != 0 ? throw new ArgumentException(errorMessage ?? $"Value ({val}) must be equal to {comparedTo}") : val;

        public static TType IsGreaterThan(TType val, TType comparedTo, string errorMessage = null)
        => val.CompareTo(comparedTo) != 1 ? throw new ArgumentException(errorMessage ?? $"Value ({val}) must be greater than {comparedTo}") : val;

        public static TType IsLessThan(TType val, TType comparedTo, string errorMessage = null)
            => val.CompareTo(comparedTo) != -1 ? throw new ArgumentException(errorMessage ?? $"Value ({val}) must be greater than {comparedTo}") : val;
    }
}