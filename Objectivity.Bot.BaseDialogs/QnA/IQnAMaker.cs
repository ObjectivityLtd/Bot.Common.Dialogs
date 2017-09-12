namespace Objectivity.Bot.BaseDialogs.QnA
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
    using Microsoft.Bot.Builder.Dialogs;

    public interface IQnAMaker
    {
        Task ReplyToChannel(IDialogContext context, string query, Func<IDialogContext, Task> noAnswerCallback);

        Task ReplyToChannel(IDialogContext context, Func<IDialogContext, Task> noAnswerCallback);

        Task ReplyToChannel(IDialogContext context, string noAnswerResponse);

        Task ReplyToChannel(IDialogContext context, string query, string noAnswerResponse);

        Task<QnAMakerResults> GetAnswers(string query);
    }
}