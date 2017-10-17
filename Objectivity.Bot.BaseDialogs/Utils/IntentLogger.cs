namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.Bot.Builder.Luis.Models;

    public class IntentLogger : IIntentLogger
    {
        public static void LogLuisResult(LuisResult result, int maxIntents, string comment)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var intentText = string.Join(
                " | ",
                result.Intents.Take(maxIntents).Select(s => $"Intent '{s.Intent}' (score {s.Score ?? 0})"));
            string entityText;
            switch (result.Entities.Count)
            {
                case 0:
                    entityText = string.Empty;
                    break;
                case 1:
                    var firstEntity = result.Entities.First();
                    entityText = $" Entity '{firstEntity.Entity}' (score {firstEntity.Score}, type {firstEntity.Type})";
                    break;
                default:
                    entityText =
                        $" Entities {string.Join(", ", result.Entities.Select(e => $"'{e.Entity}' (score {e.Score}, type {e.Type})"))}";
                    Trace.TraceWarning("More than one entity!");
                    break;
            }

            Trace.TraceInformation($"{intentText}{entityText} recognized for text '{result.Query}' comment: {comment}");
        }

        void IIntentLogger.LogLuisResult(LuisResult result, int maxIntentsToDescribe, string comment)
        {
            LogLuisResult(result, maxIntentsToDescribe, comment);
        }
    }
}