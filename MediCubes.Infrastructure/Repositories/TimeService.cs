using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class TimeService:ITimeService
    {
        private readonly ILogger<TimeService> _logger;

        public TimeService(ILogger<TimeService> logger)
        {
            _logger = logger;
        }

        public DateTime GetDateTime(DateTime utcDateTime, string? timezone)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("The input DateTime must be in UTC.");
            }
            TimeZoneInfo timeZoneInfo;
            try
            {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            }
            catch (TimeZoneNotFoundException)
            {
                _logger.LogWarning("Timezone '{Timezone}' not found. Returning UTC time.", timezone);
                return utcDateTime;
            }
            catch (InvalidTimeZoneException)
            {
                _logger.LogWarning("Timezone '{Timezone}' is invalid. Returning UTC time.", timezone);
                return utcDateTime;
            }
            DateTime convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
            return convertedDateTime;
        }
    }
}
