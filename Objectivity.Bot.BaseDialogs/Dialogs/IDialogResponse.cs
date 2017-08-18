namespace Bot.BaseDialogs.Dialogs
{
    public interface IDialogResponse<T>
    {
        T Response { get; set; }

        ResponseType ResponseType { get; set; }
    }
}