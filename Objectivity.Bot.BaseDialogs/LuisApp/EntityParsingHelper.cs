namespace Objectivity.Bot.BaseDialogs.LuisApp
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;
    using Microsoft.Bot.Builder.Luis.Models;
    using Newtonsoft.Json.Linq;
    using Objectivity.Bot.Utils;

    public static class EntityParsingHelper
    {
        public static DateTime? ToDate(this EntityRecommendation entityRecommendation)
        {
            if (entityRecommendation == null)
            {
                throw new ArgumentNullException(nameof(entityRecommendation));
            }

            if (!entityRecommendation.Resolution.Any())
            {
                return null;
            }

            object jarray;
            if (!entityRecommendation.Resolution.TryGetValue(EntityResolutionTypes.Values, out jarray))
            {
                return null;
            }
            DateTime date;
            var isDateParsable = DateTime.TryParse(entityRecommendation.Entity, out date);
            var dates = JArray.FromObject(jarray).Select(token => DateParser.ParseDate(token["value"].ToString(), isDateParsable));
            // Luis treats next year's months from today, and returns 2 dates in response, one from last year, second from next one.
            return DateTime.Today <= dates.FirstOrDefault() ? dates.FirstOrDefault() : dates.LastOrDefault();
        }

        public static TimeSpan? ToTimeSpan(this EntityRecommendation entityRecommendation)
        {
            if (entityRecommendation == null)
            {
                throw new ArgumentNullException(nameof(entityRecommendation));
            }

            if (!entityRecommendation.Resolution.Any())
            {
                return null;
            }

            object timespanString;
            if (!entityRecommendation.Resolution.TryGetValue(EntityResolutionTypes.Duration, out timespanString))
            {
                return null;
            }

            try
            {
                var match = Regex.Match(timespanString.ToString(), @"^P(?<weeksNumber>\d+)W$");
                if (match.Success)
                {
                    var weeksNumber = int.Parse(match.Groups["weeksNumber"].Value);
                    return TimeSpan.FromDays(weeksNumber * 7);
                }

                // implements ISO8601 parser for PT4H, PT1H20M, PT20M, P1D etc
                return XmlConvert.ToTimeSpan(timespanString.ToString());
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}