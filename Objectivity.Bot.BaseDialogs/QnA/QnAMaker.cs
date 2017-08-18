namespace Objectivity.Bot.BaseDialogs.QnA
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
    using Microsoft.Bot.Builder.Dialogs;

    public class QnAMaker : IQnAMaker
    {
        private readonly IQnAService qnAService;

        public QnAMaker(IQnAService qnAService)
        {
            this.qnAService = qnAService;
        }

        public async Task ReplyToChannel(IDialogContext context, string query, Func<IDialogContext, Task> noAnswerCallback)
        {
            var result = await this.GetResultFromQnAMaker(query);
            if (result != null && result.Answers.Any())
            {
                await context.PostAsync(result.Answers.FirstOrDefault()?.Answer);
            }
            else
            {
                await noAnswerCallback(context);
            }
        }

        public async Task ReplyToChannel(IDialogContext context, Func<IDialogContext, Task> noAnswerCallback)
        {
            var query = context.Activity.AsMessageActivity().Text;
            if (!string.IsNullOrEmpty(query))
            {
                await this.ReplyToChannel(context, query, noAnswerCallback);
            }
        }

        public async Task ReplyToChannel(IDialogContext context, string noAnswerResponse) => await this.ReplyToChannel(context,
            async ctx => await ctx.PostAsync(noAnswerResponse));

        public async Task ReplyToChannel(IDialogContext context, string query, string noAnswerResponse) => await this.ReplyToChannel(
            context, query, async ctx => await ctx.PostAsync(noAnswerResponse));

        private async Task<QnAMakerResults> GetResultFromQnAMaker(string query)
        {
            QnAMakerRequestBody requestBody;
            string subscriptionid;
            var uri = this.qnAService.BuildRequest(query, out requestBody, out subscriptionid);
            return await this.qnAService.QueryServiceAsync(uri, requestBody, subscriptionid);
        }
    }
}