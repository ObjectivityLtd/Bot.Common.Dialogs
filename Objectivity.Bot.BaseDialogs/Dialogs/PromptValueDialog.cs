namespace Objectivity.Bot.BaseDialogs.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;

    [Serializable]
    public abstract class PromptValueDialog<T> : BaseLuisDialog<T>
    {
        public string PromptMessage { get; set; }

        /// <summary>
        /// This method should be called when Intent is received.
        /// </summary>
        public virtual async Task HandleIntent(IDialogContext context, LuisResult result)
        {
            var value = this.GetValue(context, result);
            if (value != null)
            {
                this.EndDialog(context, value);
            }
            else
            {
                await this.PostAndWaitAsync(context, this.UnrecognizedAnswerMessage);
            }
        }

        public override async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(this.PromptMessage);
            context.Wait(this.MessageReceived);
        }

        protected abstract T GetValue(IDialogContext context, LuisResult result);
    }
}