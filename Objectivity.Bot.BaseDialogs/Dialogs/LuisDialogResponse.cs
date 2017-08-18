namespace Bot.BaseDialogs.Dialogs
{
    using Microsoft.Bot.Builder.Luis.Models;

    public class LuisDialogResponse<T> : ILuisDialogResponse<T>
    {
        public T Response { get; set; }

        public ResponseType ResponseType { get; set; }

        public LuisResult LuisResult { get; set; }

        public string Intent { get; set; }
    }
}
