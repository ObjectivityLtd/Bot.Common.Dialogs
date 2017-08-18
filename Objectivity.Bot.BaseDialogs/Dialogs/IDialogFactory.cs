namespace Bot.BaseDialogs.Dialogs
{
    using Autofac.Core;

    public interface IDialogFactory
    {
        T DialogFor<T>(params Parameter[] parameters);
    }
}