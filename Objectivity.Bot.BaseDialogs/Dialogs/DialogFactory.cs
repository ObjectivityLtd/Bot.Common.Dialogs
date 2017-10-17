namespace Objectivity.Bot.BaseDialogs.Dialogs
{
    using System;

    using Autofac;
    using Autofac.Core;

    using Objectivity.Bot.BaseDialogs.Utils;

    public class DialogFactory : IDialogFactory
    {
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
                ex.TraceError();
                throw;
            }
        }
    }
}