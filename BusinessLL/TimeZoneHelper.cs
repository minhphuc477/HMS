using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLL
{
    public static class TimeZoneHelper
    {
       
        

        public static DateTime ConvertUtcToLocal(DateTime utcDateTime, TimeSpan utcOffset)
        {
            try
            {
                // Adjust UTC time to local time using a zero offset
                var localDateTime = utcDateTime + utcOffset;
                Log.Information("Converted UTC {UtcDateTime} to Local {LocalDateTime} with zero offset", utcDateTime, localDateTime);
                return localDateTime;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error converting UTC to Local");
                throw;
            }
        }

        public static DateTime ConvertLocalToUtc(DateTime localDateTime, TimeSpan utcOffset)
        {
            try
            {
                // Adjust local time to UTC using a zero offset
                var utcDateTime = localDateTime - utcOffset;
                Log.Information("Converted Local {LocalDateTime} to UTC {UtcDateTime} with zero offset", localDateTime, utcDateTime);
                return utcDateTime;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error converting Local to UTC");
                throw;
            }
        }

    }


}
