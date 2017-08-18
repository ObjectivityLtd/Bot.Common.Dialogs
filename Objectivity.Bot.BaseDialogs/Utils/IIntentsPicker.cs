namespace Bot.BaseDialogs.Utils
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;

    public interface IIntentsPicker
    {
        Task PickCorrectIntent(IDialogContext context, LuisResult result,
            Func<IDialogContext, LuisResult, Task> callbackAction);
    }
}