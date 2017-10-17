namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.Bot.Builder.Luis.Models;

    using Newtonsoft.Json;

    using Objectivity.Bot.BaseDialogs.LuisApp;

    public static class LuisResultExtensions
    {
        public static LuisResult CopySettingNewIntent(this LuisResult luisResult, string intent, double score = 1)
        {
            var copiedLuisResult = luisResult.ToJson().ToLuisResult();

            var intentRecommendation = new IntentRecommendation(intent, score);
            copiedLuisResult.TopScoringIntent = intentRecommendation;
            copiedLuisResult.Intents = new[] { intentRecommendation };

            return copiedLuisResult;
        }

        public static IntentRecommendation GetStrongestIntent(this LuisResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var intent = result.TopScoringIntent ?? result.Intents?.FirstOrDefault();
            if (intent == null)
            {
                Trace.TraceWarning("Neither TopScoringIntent nor Intents set.");
                return new IntentRecommendation { Intent = Intents.None, Score = 1.0 };
            }

            return intent;
        }

        public static string ToJson(this LuisResult result)
        {
            return JsonConvert.SerializeObject(result);
        }

        public static LuisResult ToLuisResult(this string resultSerialized)
        {
            return JsonConvert.DeserializeObject<LuisResult>(resultSerialized);
        }
    }
}