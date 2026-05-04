using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankUPG.Infrastructure.Data
{
    public class DateTimeService
    {
        private static readonly TimeZoneInfo IstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        /// <summary>
        /// Gets the current date and time in IST (Indian Standard Time)
        /// </summary>
        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IstTimeZone);

        /// <summary>
        /// Gets the current date in IST (Indian Standard Time) with time set to midnight
        /// </summary>
        public static DateTime Today => Now.Date;

        /// <summary>
        /// Converts a UTC DateTime to IST
        /// </summary>
        public static DateTime UtcToIst(DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Local)
                utcDateTime = utcDateTime.ToUniversalTime();

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, IstTimeZone);
        }

        /// <summary>
        /// Converts an IST DateTime to UTC
        /// </summary>
        public static DateTime IstToUtc(DateTime istDateTime)
        {
            if (istDateTime.Kind == DateTimeKind.Utc)
                return istDateTime;

            return TimeZoneInfo.ConvertTimeToUtc(istDateTime, IstTimeZone);
        }

        /// <summary>
        /// Converts any DateTime to IST
        /// </summary>
        public static DateTime ToIst(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, IstTimeZone);

            if (dateTime.Kind == DateTimeKind.Local)
                return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Local, IstTimeZone);

            return dateTime;
        }
    }
}
