namespace Objectivity.Bot.BaseDialogs.AutofacModules
{
    using System.Configuration;
    using Autofac;
    using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
    using QnA;

    public class QnAModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var subscriptionKey = ConfigurationManager.AppSettings["QnASubscriptionKey"];
            var knowledgebaseId = ConfigurationManager.AppSettings["QnAKnowledgebaseId"];

            builder.RegisterType<QnAMakerService>().As<IQnAService>().SingleInstance()
                .WithParameter("qnaInfo", new QnAMakerAttribute(subscriptionKey, knowledgebaseId));

            builder.RegisterType<QnAMaker>().As<IQnAMaker>();
        }
    }
}
