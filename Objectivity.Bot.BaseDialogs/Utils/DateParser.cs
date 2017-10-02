namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    public static class DateParser
    {
        public static bool IsStringContainsDotsInDate(string date)
        {
            return Regex.IsMatch(date, @"\d\d.\d\d.\d\d\d\d");
        }

        public static DateTime ParseDate(string dateValue, bool isParsable = true)
        {
            if (dateValue == null)
            {
                throw new ArgumentNullException(nameof(dateValue));
            }

            var today = DateTime.Now.Date;

            // "XXXX-30-09" - "september 30th",
            if (RegexHelper.GetRegex(EnumRegexTypes.MonthAndDayMatch).IsMatch(dateValue))
            {
                return ParseDate(dateValue.Replace("XXXX", today.Year.ToString()), isParsable);
            }

            // "2015-09-30"
            if (TryParseDateWithPriorFormat(dateValue, out DateTime date, isParsable ? null : "yyyy-MM-dd"))
            {
                return date;
            }

            // "2015-W34" - next week
            var weekNumberMatch = RegexHelper.GetRegex(EnumRegexTypes.WeekNumberMatch).Match(dateValue);
            if (weekNumberMatch.Success)
            {
                int.TryParse(weekNumberMatch.Groups["year"].Value, out int year);
                int.TryParse(weekNumberMatch.Groups["weekNumber"].Value, out int weekNumber);
                return FirstDateOfWeek(year, weekNumber, new CultureInfo("pl-PL")); // first monday
            }

            // "XXXX-WXX-1" get monday
            var dayOfTheWeekMatch = RegexHelper.GetRegex(EnumRegexTypes.DayOfTheWeekMatch).Match(dateValue);
            if (dayOfTheWeekMatch.Success)
            {
                int.TryParse(dayOfTheWeekMatch.Groups["dayOfWeek"].Value, out int desiredDayOfWeek);
                var todayDayOfWeek = (int)today.DayOfWeek;
                return today.AddDays(desiredDayOfWeek - todayDayOfWeek);
            }

            // "XXXX-11" or "2017-11" get november
            var monthOfTheYearMatch = RegexHelper.GetRegex(EnumRegexTypes.MonthOfTheYearMatch).Match(dateValue);
            if (monthOfTheYearMatch.Success)
            {
                if (!int.TryParse(weekNumberMatch.Groups["year"].Value, out int year))
                {
                    year = DateTime.Now.Year;
                }

                if (int.TryParse(monthOfTheYearMatch.Groups["monthOfTheYear"].Value, out int desiredMonthOfTheYear))
                {
                    return desiredMonthOfTheYear > DateTime.Now.Month
                               ? new DateTime(year, desiredMonthOfTheYear, 1)
                               : DateTime.Now;
                }
            }

            return today;
        }

        public static string ParseDotsToDashes(string date)
        {
            var pattern = "(?<day>\\d{1,2}).\\b(?<month>\\d{1,2}).(?<year>\\d{2,4})\\b";
            var newPattern = @"${day}-${month}-${year}";
            var output = Regex.Replace(date, pattern, newPattern, RegexOptions.None);
            return output;
        }

        private static DateTime FirstDateOfWeek(int year, int weekOfYear, CultureInfo ci)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = (int)ci.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
            var firstWeekDay = jan1.AddDays(daysOffset);
            var firstWeek = ci.Calendar.GetWeekOfYear(
                jan1,
                ci.DateTimeFormat.CalendarWeekRule,
                ci.DateTimeFormat.FirstDayOfWeek);
            if ((firstWeek <= 1 || firstWeek >= 52) && daysOffset >= -3)
            {
                weekOfYear -= 1;
            }

            return firstWeekDay.AddDays(weekOfYear * 7);
        }

        private static bool TryParseDateWithPriorFormat(string date, out DateTime dateTime, string format = null)
        {
            if (!DateTime.TryParseExact(
                    date,
                    format ?? "yyyy-dd-MM",
                    CultureInfo.CurrentCulture,
                    DateTimeStyles.None,
                    out DateTime tempDate))
            {
                DateTime.TryParse(date, out tempDate);
            }

            dateTime = tempDate;
            return tempDate != default(DateTime);
        }
    }
}