namespace Bot.BaseDialogs.Utils
{
    using System.Collections.Generic;
    using Microsoft.Bot.Builder.Luis.Models;

    public interface IIntentLogger
    {
        void LogLuisResult(LuisResult result, int maxIntents = 1, string comment = "");
    }
}