using System;

namespace XRMFramework.Util.Validation
{
    public static class DateTimeValidations
    {
        /// <summary>
        /// Compares a value and make sure that at least the specified number of months has passed
        /// </summary>
        /// <param name="dateTime">DateTime to compare. Will be compared agains DateTime.Now.Date</param>
        /// <param name="months">Number of months that must have passed</param>
        /// <param name="errorMessage">An optional error message to use as exception message when validation fails.</param>
        /// <exception cref="ArgumentException">Will throw exception if condition is not meet</exception>
        /// <returns></returns>
        public static DateTime NumberOfMonthsMustHavePassed(this DateTime dateTime, int months, string errorMessage = null)
            => ValueTypeComparisons<DateTime>.IsEqualOrLessThan(dateTime.Date, DateTime.Now.Date.AddMonths(-1 * months), errorMessage);

        public static DateTime TimeMustHavePassed(this DateTime date, TimeSpan timeThatMustHavePassed, string errorMessage = null)
        {
            if (date.Add(timeThatMustHavePassed) >= DateTime.Now)
            {
                throw new ArgumentOutOfRangeException(errorMessage ?? "The specified amount of time has not elapsed");
            }

            return date;
        }
    }
}