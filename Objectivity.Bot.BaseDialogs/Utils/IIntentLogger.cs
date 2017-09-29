﻿namespace Objectivity.Bot.BaseDialogs.Utils
{
    using Microsoft.Bot.Builder.Luis.Models;

    public interface IIntentLogger
    {
        void LogLuisResult(LuisResult result, int maxIntents = 1, string comment = "");
    }
}