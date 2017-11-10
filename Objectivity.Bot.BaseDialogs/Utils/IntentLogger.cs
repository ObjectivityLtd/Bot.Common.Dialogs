namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System;
    using System.Globalization;
    using System.Linq;

    using Microsoft.Bot.Builder.Luis.Models;

    using NLog;

    public class IntentLogger : IIntentLogger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void LogLuisResult(LuisResult result, int maxIntents, string comment)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var intentText = string.Join(
                " | ",
                result.Intents.Take(maxIntents).Select(s =>
                    string.Format(CultureInfo.InvariantCulture, "Intent '{0}' (score {1})", s.Intent, s.Score ?? 0)));
            string entityText;
            switch (result.Entities.Count)
            {
                case 0:
                    entityText = string.Empty;
                    break;
                case 1:
                    var firstEntity = result.Entities.First();
                    entityText = string.Format(
                        CultureInfo.InvariantCulture,
                        " Entity '{0}' (score {1}, type {2})",
                        firstEntity.Entity,
                        firstEntity.Score,
                        firstEntity.Type);
                    break;
                default:
                    entityText =
                        string.Format(
                            CultureInfo.InvariantCulture,
                            " Entities {0}",
                            string.Join(", ", result.Entities.Select(e => string.Format(CultureInfo.InvariantCulture, "'{0}' (score {1}, type {2})", e.Entity, e.Score, e.Type))));
                    Logger.Warn("More than one entity!");
                    break;
            }

            Logger.Debug(CultureInfo.InvariantCulture, "{0}{1} recognized for text '{2}' comment: {3}", intentText, entityText, result.Query, comment);
        }

        void IIntentLogger.LogLuisResult(LuisResult result, int maxIntentsToDescribe, string comment)
        {
            LogLuisResult(result, maxIntentsToDescribe, comment);
        }
    }
}