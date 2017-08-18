namespace Bot.BaseDialogs.Utils
{
    using System;
    using System.Collections.Generic;
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

            var intentText = string.Join(" | ", result.Intents.Take(maxIntents).Select(s => $"Intent '{s.Intent}' (score {s.Score ?? 0})"));
            string entityText;
            switch (result.Entities.Count)
            {
                case 0:
                    entityText = string.Empty;
                    break;
                case 1:
                    var firstEntity = result.Entities.First();
                    entityText =
                        $" Entity '{firstEntity.Entity}' (score {firstEntity.Score}, type {firstEntity.Type})";
                    break;
                default:
                    entityText =
                        $" Entities {string.Join(", ", result.Entities.Select(e => $"'{e.Entity}' (score {e.Score}, type {e.Type})"))}";
                    IntentLogger.Logger.Warn("More than one entity!");
                    break;
            }

            IntentLogger.Logger.Debug($"{intentText}{entityText} recognized for text '{result.Query}' comment: {comment}");
        }

        void IIntentLogger.LogLuisResult(LuisResult result, int maxIntentsToDescribe, string comment)
        {
            IntentLogger.LogLuisResult(result, maxIntentsToDescribe, comment);
        }
    }
}