namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public enum EnumRegexTypes
    {
        WeekNumberMatch,

        DayOfTheWeekMatch,

        MonthOfTheYearMatch,

        ExactDateMatch,

        MonthAndDayMatch
    }

    public static class RegexHelper
    {
        private static readonly Dictionary<EnumRegexTypes, string> RegexDictionary =
            new Dictionary<EnumRegexTypes, string>
                {
                    {
                        EnumRegexTypes.DayOfTheWeekMatch,
                        @"XXXX-WXX-(?<dayOfWeek>\d+)"
                    },
                    {
                        EnumRegexTypes.WeekNumberMatch,
                        @"(?<year>\d+)-W(?<weekNumber>\d+)"
                    },
                    {
                        EnumRegexTypes.MonthOfTheYearMatch,
                        @"(?<year>XXXX|\d\d\d\d)-(?<monthOfTheYear>\d+)"
                    },
                    { EnumRegexTypes.ExactDateMatch, @"\d\d\d\d-\d\d-\d\d" },
                    { EnumRegexTypes.MonthAndDayMatch, @"XXXX-\d+-\d+" }
                };

        public static Regex GetRegex(EnumRegexTypes type)
        {
            return new Regex(RegexDictionary[type]);
        }
    }
}