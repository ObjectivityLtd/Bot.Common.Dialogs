namespace Bot.BaseDialogs.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using global::Microsoft.Bot.Builder.Dialogs;
    using global::Microsoft.Bot.Connector;
    using Services;
    using Utils;

    [Serializable]
    public class SkippablePromptConfirm : PromptDialog.PromptConfirm
    {
        private readonly BaseLuisDialog<object> baseLuisDialog;
        private readonly ILuisServiceProvider luisServiceProvider;

        public SkippablePromptConfirm(BaseLuisDialog<object> baseLuisDialog, ILuisServiceProvider luisServiceProvider, string prompt, string retry, int attempts,
            PromptStyle promptStyle = PromptStyle.Auto, string[] options = null, string[][] patterns = null) : base(
            prompt, retry, attempts, promptStyle, options, patterns)
        {
            this.baseLuisDialog = baseLuisDialog;
            this.luisServiceProvider = luisServiceProvider;
        }

        public SkippablePromptConfirm(BaseLuisDialog<object> baseLuisDialog, ILuisServiceProvider luisServiceProvider, IPromptOptions<string> promptOptions,
            string[][] patterns = null) : base(promptOptions, patterns)
        {
            this.baseLuisDialog = baseLuisDialog;
            this.luisServiceProvider = luisServiceProvider;
        }

        protected override async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            bool result;
            var messageActivity = await message;
            var tryParse = this.TryParse(messageActivity, out result);

            if (tryParse)
            {
                await base.MessageReceivedAsync(context, message);
            }
            else
            {
                var forwarded = await new SkippableDialogForwarder(this.baseLuisDialog, this.luisServiceProvider).TryToForward(context, messageActivity);
                if (!forwarded)
                {
                    await base.MessageReceivedAsync(context, message);
                }
            }
        }


        public static void Confirm(BaseLuisDialog<object> basedialog, ILuisServiceProvider luisServiceProvider, IDialogContext context, ResumeAfter<bool> resume,
            string prompt, string retry = null, int attempts = 3, PromptStyle promptStyle = PromptStyle.Auto,
            string[] options = null, string[][] patterns = null)
        {
            var promptOptions = new PromptOptions<string>(prompt, retry, attempts: attempts,
                options: options ?? PromptDialog.PromptConfirm.Options,
                promptStyler: new PromptStyler(promptStyle: promptStyle));

            var child = new SkippablePromptConfirm(basedialog, luisServiceProvider, promptOptions, patterns);
            context.Call(child, resume);
        }
    }
}