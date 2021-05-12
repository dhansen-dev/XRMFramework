using System;

namespace XRMFramework.Core
{
    public class DateTimeProvider : IDateTimeProvider
    {
        // Returns DateTime.UtcNow (DateTimeKind.Utc)
        public DateTime CurrentUTC => DateTime.UtcNow;

        /// <summary>
        /// Returns DateTime.Now (DateTimeKind.Local). When using CRM Online this is the servers local time
        /// and not the current users.
        /// </summary>
        ///
        public DateTime CurrentLocal => CurrentUTC.ToLocalTime();

        /// <summary>
        /// Returns DateTime.Now but with DateTime.Kind set to unspecified to avoid time zone issues
        /// with CRM Server when using date fields were we are only interested to set the date time without
        /// any conversions.
        /// </summary>
        public DateTime Current
            => DateTime.SpecifyKind(CurrentLocal, DateTimeKind.Unspecified);

        /// <summary>
        /// Returns Date part of Current (DateTime.Kind set to unspecified)
        /// </summary>
        public DateTime Today => Current.Date;
    }

    public interface IDateTimeProvider
    {
        DateTime CurrentUTC { get; }
        DateTime CurrentLocal { get; }
        DateTime Current { get; }
        DateTime Today { get; }
    }
}