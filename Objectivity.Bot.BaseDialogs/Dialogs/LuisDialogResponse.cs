namespace Objectivity.Bot.BaseDialogs.Dialogs
{
    using Microsoft.Bot.Builder.Luis.Models;

    public class LuisDialogResponse<T> : ILuisDialogResponse<T>
    {
        public string Intent { get; set; }

        public LuisResult LuisResult { get; set; }

        public T Response { get; set; }

        public ResponseType ResponseType { get; set; }
    }
}