namespace Objectivity.Bot.BaseDialogs.Dialogs
{
    using System;
    using Microsoft.Bot.Builder.Luis.Models;

    [Serializable]
    public class LuisPromptResult
    {
        public LuisPromptResultType ResultType { get; set; }

        public LuisResult LuisResult { get; set; }
    }
}
