namespace Objectivity.Bot.BaseDialogs.AutofacModules
{
    using System.Configuration;
    using System.Linq;

    using Autofac;

    using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Internals.Fibers;

    using Objectivity.Bot.BaseDialogs.Dialogs;
    using Objectivity.Bot.BaseDialogs.QnA;
    using Objectivity.Bot.BaseDialogs.Services;
    using Objectivity.Bot.BaseDialogs.Utils;

    public class BaseDialogsModule : Module
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is autofac container builder")]
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(this.GetType().Assembly)
                .Where(
                    t => t.GetInterfaces().Where(i => i.IsGenericType)
                        .Any(i => i.GetGenericTypeDefinition() == typeof(IDialog<>))).AsSelf().PropertiesAutowired();

            builder.RegisterType<DialogFactory>().Keyed<IDialogFactory>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<AppConfigLuisServiceProvider>()
                .Keyed<ILuisServiceProvider>(FiberModule.Key_DoNotSerialize).As<ILuisServiceProvider>();

            builder.RegisterType<IntentDescriptionProvider>()
                .Keyed<IIntentDescriptionProvider>(FiberModule.Key_DoNotSerialize).As<IIntentDescriptionProvider>();

            builder.RegisterType<IntentsPicker>().As<IIntentsPicker>();

            builder.RegisterType<QnAMakerService>().As<IQnAService>().SingleInstance().WithParameter(
                "qnaInfo",
                new QnAMakerAttribute(
                    ConfigurationManager.AppSettings["QnASubscriptionKey"],
                    ConfigurationManager.AppSettings["QnAKnowledgebaseId"]));

            builder.RegisterType<QnAMaker>().As<IQnAMaker>();

            builder.RegisterType<ApplicationSettings>().AsImplementedInterfaces().SingleInstance();
        }
    }
}