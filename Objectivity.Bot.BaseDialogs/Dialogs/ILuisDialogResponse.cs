namespace Bot.BaseDialogs.Dialogs
{
    using Microsoft.Bot.Builder.Luis.Models;

    public interface ILuisDialogResponse<T> : IDialogResponse<T>
    {
        LuisResult LuisResult { get; set; }

        string Intent { get; set; }
    }
}