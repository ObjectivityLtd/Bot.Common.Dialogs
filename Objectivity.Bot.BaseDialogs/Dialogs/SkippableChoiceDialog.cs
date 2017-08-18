namespace Bot.BaseDialogs.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Services;
    using Utils;

    [Serializable]
    public class SkippableChoiceDialog<T> :  PromptDialog.PromptChoice<T>
    {
        private readonly BaseLuisDialog<object> baseLuisDialog;
        private readonly ILuisServiceProvider luisServiceProvider;

        public SkippableChoiceDialog(BaseLuisDialog<object> baseLuisDialog, ILuisServiceProvider luisServiceProvider, IEnumerable<T> options, string prompt, string retry, int attempts, PromptStyle promptStyle =  PromptStyle.Auto, IEnumerable<string> descriptions = null, bool recognizeChoices = true, bool recognizeNumbers = true, bool recognizeOrdinals = true, double minScore = 0.4) : base(options, prompt, retry, attempts, promptStyle, descriptions, recognizeChoices, recognizeNumbers, recognizeOrdinals, minScore)
        {
            this.baseLuisDialog = baseLuisDialog;
            this.luisServiceProvider = luisServiceProvider;
        }

        public SkippableChoiceDialog(BaseLuisDialog<object> baseLuisDialog, ILuisServiceProvider luisServiceProvider, IDictionary<T, IEnumerable<T>> choices, string prompt, string retry, int attempts, PromptStyle promptStyle =  PromptStyle.Auto, IEnumerable<string> descriptions = null, bool recognizeChoices = true, bool recognizeNumbers = true, bool recognizeOrdinals = true, double minScore = 0.4) : base(choices, prompt, retry, attempts, promptStyle, descriptions, recognizeChoices, recognizeNumbers, recognizeOrdinals, minScore)
        {
            this.baseLuisDialog = baseLuisDialog;
            this.luisServiceProvider = luisServiceProvider;
        }

        public SkippableChoiceDialog(BaseLuisDialog<object> baseLuisDialog,  ILuisServiceProvider luisServiceProvider, IPromptOptions<T> promptOptions, bool recognizeChoices = true, bool recognizeNumbers = true, bool recognizeOrdinals = true, double minScore = 0.4) : base(promptOptions, recognizeChoices, recognizeNumbers, recognizeOrdinals, minScore)
        {
            this.baseLuisDialog = baseLuisDialog;
            this.luisServiceProvider = luisServiceProvider;
        }

        public static void Choice(BaseLuisDialog<object> basedialog, ILuisServiceProvider luisServiceProvider, IDialogContext context, ResumeAfter<T> resume, IEnumerable<T> options, string prompt, string retry = null, int attempts = 3)
        {
            var promptChoice = new SkippableChoiceDialog<T>(basedialog, luisServiceProvider, options, prompt, retry, attempts);
            context.Call(promptChoice, resume);
        }

        protected override async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            T result;
            if (this.TryParse(await message, out result))
            {
                await base.MessageReceivedAsync(context, message);
            }
            else
            {
                var forwarded = await new SkippableDialogForwarder(this.baseLuisDialog, this.luisServiceProvider).TryToForward(context, await message);
                if (!forwarded)
                {
                    await base.MessageReceivedAsync(context, message);
                }
            }
        }
    }
}