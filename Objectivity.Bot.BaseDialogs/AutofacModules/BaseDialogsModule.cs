namespace Bot.BaseDialogs.AutofacModules
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Autofac;
    using Autofac.Core;
    using Bot.BaseDialogs.Dialogs;
    using Bot.BaseDialogs.Services;
    using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Utils;

    public class BaseDialogsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(this.GetType().Assembly)
                .Where(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Any(i => i.GetGenericTypeDefinition() == typeof(IDialog<>)))
                .AsSelf()
                .PropertiesAutowired();

            builder.RegisterType<DialogFactory>()
                .Keyed<IDialogFactory>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<AppConfigLuisServiceProvider>()
                .Keyed<ILuisServiceProvider>(FiberModule.Key_DoNotSerialize)
                .As<ILuisServiceProvider>();

            builder.RegisterType<IntentDescriptionProvider>()
                .Keyed<IIntentDescriptionProvider>(FiberModule.Key_DoNotSerialize).As<IIntentDescriptionProvider>();

            builder.RegisterType<IntentsPicker>()
                .As<IIntentsPicker>();

            builder.RegisterType<QnAMakerService>().As<IQnAService>().SingleInstance()
                .WithParameter(
                    "qnaInfo",
                    new QnAMakerAttribute(
                        ConfigurationManager.AppSettings["QnASubscriptionKey"],
                        ConfigurationManager.AppSettings["QnAKnowledgebaseId"]));

            builder.RegisterType<QnAMaker>().As<IQnAMaker>();

            builder.RegisterType<ApplicationSettings>().AsImplementedInterfaces().SingleInstance();
        }
    }
}
