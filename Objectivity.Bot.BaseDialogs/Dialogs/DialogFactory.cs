namespace Objectivity.Bot.BaseDialogs.Dialogs
{
    using System;
    using Autofac;
    using Autofac.Core;
    using NLog;

    public class DialogFactory : IDialogFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IComponentContext ctx;

        public DialogFactory(IComponentContext ctx)
        {
            this.ctx = ctx;
        }

        public T DialogFor<T>(params Parameter[] parameters)
        {
            try
            {
                return this.ctx.Resolve<T>(parameters);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }
    }
}