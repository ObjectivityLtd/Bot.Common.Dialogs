namespace Bot.BaseDialogs.AutofacModules
{
    using Autofac;
    using Microsoft.Bot.Builder.Autofac.Base;
    using Microsoft.Bot.Builder.Dialogs.Internals;

    public class BotFrameworkUtilsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<CustomPostUnhandledExceptionToUser>().Keyed<IPostToBot>(typeof(CustomPostUnhandledExceptionToUser)).InstancePerLifetimeScope();
            builder.RegisterAdapterChain<IPostToBot>(
                typeof(EventLoopDialogTask),
                typeof(SetAmbientThreadCulture),
                typeof(PersistentDialogTask),
                typeof(ExceptionTranslationDialogTask),
                typeof(SerializeByConversation),
                typeof(CustomPostUnhandledExceptionToUser),
                typeof(LogPostToBot)
            ).InstancePerLifetimeScope();
        }
    }
}