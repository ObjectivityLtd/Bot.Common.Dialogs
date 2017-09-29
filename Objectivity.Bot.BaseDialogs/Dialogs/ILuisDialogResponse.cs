namespace Objectivity.Bot.BaseDialogs.Dialogs
{
    using Microsoft.Bot.Builder.Luis.Models;

    public interface ILuisDialogResponse<T> : IDialogResponse<T>
    {
        string Intent { get; set; }

        LuisResult LuisResult { get; set; }
    }
}