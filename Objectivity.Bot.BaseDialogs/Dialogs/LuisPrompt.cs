namespace Objectivity.Bot.BaseDialogs.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LuisApp;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using NLog;
    using Services;
    using Utils;

    /// <summary>
    /// Equivalent of <see cref="PromptDialog.PromptConfirm"/> but allowing more than yes/no responses
    /// </summary>
    [Serializable]
    public class LuisPrompt : IDialog<LuisPromptResult>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly ILuisService[] luisServices;
        private readonly string prompt;
        private readonly string[] luisIntents;
        private readonly string retry;
        private readonly BooleanRecognizer recognizer;
        private readonly bool isLuisUsedFirst;

        /// <summary>
        /// What to display when user didn't say a valid response after <see cref="attempts"/>.
        /// </summary>
        private readonly string tooManyAttempts;

        /// <summary>
        /// Maximum number of attempts.
        /// </summary>
        private int attempts;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisPrompt"/> class.
        /// </summary>
        /// <param name="luisServices">LUIS services interpreting responses other than yes/no</param>
        /// <param name="prompt">The prompt.</param>
        /// <param name="attempts">Maximum number of attempts.</param>
        /// <param name="retry">What to show on retry.</param>
        /// <param name="luisIntents">Intents for luis to limit query</param>
        /// <param name="patterns">Yes and no alternatives for matching input where first dimension is either <see cref="PromptDialog.PromptConfirm.Yes"/> or <see cref="PromptDialog.PromptConfirm.No"/> and the arrays are alternative strings to match.</param>
        /// <param name="tooManyAttempts">What to display when user didn't say a valid response after <see cref="attempts"/>.</param>
        /// <param name="isLuisUsedFirst">Whether Luis or regular prompt should be checked first.</param>
        private LuisPrompt(ILuisService[] luisServices, string prompt, int attempts, string retry = null, string[] luisIntents = null, string[][] patterns = null, string tooManyAttempts = null, bool isLuisUsedFirst = true)
        {
            this.luisServices = luisServices;
            this.prompt = prompt;
            this.attempts = attempts;
            this.retry = retry ?? new PromptDialog.PromptConfirm(this.prompt, this.retry, this.attempts).DefaultRetry; //just to steal retry message;
            this.luisIntents = luisIntents;
            this.recognizer = new BooleanRecognizer(patterns);
            this.tooManyAttempts = tooManyAttempts ?? Microsoft.Bot.Builder.Resource.Resources.TooManyAttempts;
            this.isLuisUsedFirst = isLuisUsedFirst;
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
        /// <param name="luisIntents">Intents for luis to limit query</param>
        /// <param name="patterns">Yes and no alternatives for matching input where first dimension is either <see cref="PromptDialog.PromptConfirm.Yes"/> or <see cref="PromptDialog.PromptConfirm.No"/> and the arrays are alternative strings to match.</param>
        /// <param name="tooManyAttempts">What to display when user didn't say a valid response after <see cref="attempts"/>.</param>
        /// <param name="isLuisUsedFirst">Whether Luis or regular prompt should be checked first.</param>
        public static void Confirm(
            ILuisService luisService,
            IDialogContext context,
            ResumeAfter<LuisPromptResult> resume,
            string prompt,
            int attempts = 3,
            string retry = null,
            string[] luisIntents = null,
            string[][] patterns = null,
            string tooManyAttempts = null,
            bool isLuisUsedFirst = true)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var child = new LuisPrompt(new[] { luisService }, prompt, attempts, retry, luisIntents, patterns, tooManyAttempts, isLuisUsedFirst);
            context.Call(child, resume);
        }

        /// <summary>
        /// Ask a yes/no question and allow additional responses
        /// </summary>
        /// <param name="luisServices">LUIS service interpreting responses other than yes/no</param>
        /// <param name="context">Dialog context</param>
        /// <param name="resume">Resume handler.</param>
        /// <param name="prompt">The prompt to show to the user.</param>
        /// <param name="attempts">The number of times to retry.</param>
        /// <param name="retry">What to display on retry.</param>
        /// <param name="luisIntents">Intents for luis to limit query</param>
        /// <param name="patterns">Yes and no alternatives for matching input where first dimension is either <see cref="PromptDialog.PromptConfirm.Yes"/> or <see cref="PromptDialog.PromptConfirm.No"/> and the arrays are alternative strings to match.</param>
        /// <param name="tooManyAttempts">What to display when user didn't say a valid response after <see cref="attempts"/>.</param>
        /// <param name="isLuisUsedFirst">Whether Luis or regular prompt should be checked first.</param>
        public static void Confirm(
            ILuisService[] luisServices,
            IDialogContext context,
            ResumeAfter<LuisPromptResult> resume,
            string prompt,
            int attempts = 3,
            string retry = null,
            string[] luisIntents = null,
            string[][] patterns = null,
            string tooManyAttempts = null,
            bool isLuisUsedFirst = true)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var child = new LuisPrompt(luisServices, prompt, attempts, retry, luisIntents, patterns, tooManyAttempts, isLuisUsedFirst);
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

        private async Task GotResponse(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var response = await argument;
            if (this.isLuisUsedFirst)
            {
                await this.UseLuis(context, response);
            }
            else
            {
                await this.UseRegularPrompt(context, response);
            }
        }

        private async Task UseRegularPrompt(IDialogContext context, IMessageActivity response)
        {
            var entity = this.recognizer.RecognizeEntity(response);

            if (entity.HasValue)
            {
                context.Done(new LuisPromptResult { ResultType = entity.Value ? LuisPromptResultType.Yes : LuisPromptResultType.No });
            }
            else if (this.isLuisUsedFirst)
            {
                await this.HandleRetry(context);
            }
            else
            {
                await this.UseLuis(context, response);
            }
        }

        private async Task UseLuis(IDialogContext context, IMessageActivity response)
        {
            var luisresult = await LuisQueryHelper.GetResultWithStrongestTopScoringIntent(this.luisServices.ToLuisServiceWrappers(), response.Text, context.CancellationToken);

            LuisPromptResult luisPromptResult = new LuisPromptResult() { ResultType = LuisPromptResultType.LuisResult };

            var isUnkownReponse =
                string.IsNullOrEmpty(luisresult.TopScoringIntent.Intent) ||
                luisresult.TopScoringIntent.Intent == Intents.None ||
                !this.luisIntents?.Contains(luisresult.TopScoringIntent.Intent) == true;

            if (!isUnkownReponse)
            {
                luisPromptResult.LuisResult = luisresult;
                context.Done(luisPromptResult);
                return;
            }

            if (!this.isLuisUsedFirst)
            {
                var noneEntityRecommendation = new EntityRecommendation(Intents.None);
                luisPromptResult.LuisResult = new LuisResult(
                    luisresult.Query,
                    new List<EntityRecommendation> { noneEntityRecommendation },
                    new IntentRecommendation(Intents.None, 1));
                context.Done(luisPromptResult);
            }
            else
            {
                await this.UseRegularPrompt(context, response);
            }
        }

        private async Task HandleRetry(IDialogContext context)
        {
            if (this.attempts-- > 0)
            {
                await context.PostAsync(this.retry);
                context.Wait(this.GotResponse);
            }
            else
            {
                await context.PostAsync(this.tooManyAttempts);
                context.Done(new LuisPromptResult { ResultType = LuisPromptResultType.TooManyAttempts });
            }
        }
    }
}