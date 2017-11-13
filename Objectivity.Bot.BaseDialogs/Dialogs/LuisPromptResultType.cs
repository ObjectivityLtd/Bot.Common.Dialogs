namespace Objectivity.Bot.BaseDialogs.Dialogs
{
    using System;

    [Serializable]
    public enum LuisPromptResultType
    {
        TooManyAttempts,
        Yes,
        No,
        LuisResult
    }
}