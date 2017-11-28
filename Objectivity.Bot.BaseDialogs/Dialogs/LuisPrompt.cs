namespace Objectivity.Bot.BaseDialogs.Dialogs
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using NLog;
    using Utils;

    /// <summary>
    /// Equivalent of <see cref="PromptDialog.PromptConfirm"/> but allowing more than yes/no responses
    /// </summary>
    public class LuisPrompt : IDialog<LuisPromptResult>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly ILuisService luisService;
        private readonly string prompt;
        private readonly string[] luisIntents;
        private readonly string retry;
        private readonly BooleanRecognizer recognizer;
        private int attempts;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisPrompt"/> class.
        /// </summary>
        /// <param name="luisService">LUIS service interpreting responses other than yes/no</param>
        /// <param name="prompt">The prompt.</param>
        /// <param name="attempts">Maximum number of attempts.</param>
        /// <param name="retry">What to show on retry.</param>
        /// <param name="luisIntents"></param>
        /// <param name="patterns">Yes and no alternatives for matching input where first dimension is either <see cref="PromptDialog.PromptConfirm.Yes"/> or <see cref="PromptDialog.PromptConfirm.No"/> and the arrays are alternative strings to match.</param>
        private LuisPrompt(ILuisService luisService, string prompt, int attempts, string retry = null, string[] luisIntents = null, string[][] patterns = null)
        {
            this.luisService = luisService;
            this.prompt = prompt;
            this.attempts = attempts;
            this.retry = retry ?? new PromptDialog.PromptConfirm(this.prompt, this.retry, this.attempts).DefaultRetry; //just to steal retry message;
            this.luisIntents = luisIntents;
            this.recognizer = new BooleanRecognizer(patterns);
        }

        /// <summary>
        /// Ask a yes/no question and allow additional responses
        /// </summary>
        /// <param name="luisService">LUIS service interpreting responses other than yes/no</param>
        /// <param name="context">Dialog context</param>
        /// <param name="resume">Resume handler.</param>
        /// <param name="prompt">The prompt to show to the user.</param>
        /// <param name="attempts">The number of times to retry.</param>
        /// <param name="retry">What to display on retry.</param>
        /// <param name="luisIntents"></param>
        /// <param name="patterns">Yes and no alternatives for matching input where first dimension is either <see cref="PromptDialog.PromptConfirm.Yes"/> or <see cref="PromptDialog.PromptConfirm.No"/> and the arrays are alternative strings to match.</param>
        public static void Confirm(
            ILuisService luisService,
            IDialogContext context,
            ResumeAfter<LuisPromptResult> resume,
            string prompt,
            int attempts = 3,
            string retry = null,
            string[] luisIntents = null,
            string[][] patterns = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var child = new LuisPrompt(luisService, prompt, attempts, retry, luisIntents, patterns);
            context.Call(child, resume);
        }

        /// <inheritdoc />
        /// <summary>
        /// Encapsulate a method that represents the code to start a dialog.
        /// </summary>
        /// <param name="context">The dialog context.</param>
        /// <returns>A task that represents the start code for a dialog.</returns>
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(this.prompt);
            context.Wait(this.GotResponse);
        }

        private async Task<LuisResult> QueryLuis(string input, CancellationToken cancellationToken)
        {
            Logger.Trace(CultureInfo.InvariantCulture, "Querying LUIS for {0}", input);
            var request = this.luisService.ModifyRequest(new LuisRequest(input));
            var result = await this.luisService.QueryAsync(request, cancellationToken);
            return result;
        }

        private async Task GotResponse(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var response = await argument;

            var luisresult = await this.QueryLuis(response.Text, context.CancellationToken);
            var isUnkownReponse =
                string.IsNullOrEmpty(luisresult.TopScoringIntent.Intent) ||
                !this.luisIntents?.Contains(luisresult.TopScoringIntent.Intent) == true;
            if (isUnkownReponse)
            {
                await this.UseRegularPrompt(context, response);
            }
            else
            {
                context.Done(new LuisPromptResult { ResultType = LuisPromptResultType.LuisResult, LuisResult = luisresult });
            }
        }

        private async Task UseRegularPrompt(IDialogContext context, IMessageActivity response)
        {
            var entity = this.recognizer.RecognizeEntity(response);

            if (entity.HasValue)
            {
                context.Done(new LuisPromptResult { ResultType = entity.Value ? LuisPromptResultType.Yes : LuisPromptResultType.No });
            }
            else
            {
                await this.HandleRetry(context);
            }
        }

        private async Task HandleRetry(IDialogContext context)
        {
            this.attempts--;

            if (this.attempts > 0)
            {
                await context.PostAsync(this.retry);
                context.Wait(this.GotResponse);
            }
            else
            {
                await context.PostAsync(Microsoft.Bot.Builder.Resource.Resources.TooManyAttempts);
                context.Done(new LuisPromptResult { ResultType = LuisPromptResultType.TooManyAttempts });
            }
        }
    }
}