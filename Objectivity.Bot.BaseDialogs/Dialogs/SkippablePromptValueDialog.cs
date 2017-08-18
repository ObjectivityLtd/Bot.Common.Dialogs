namespace Objectivity.Bot.BaseDialogs.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using LuisApp;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using Services;
    using Utils;

    [Serializable]
    public class SkippableDateDialog : DateDialog
    {
        private readonly BaseLuisDialog<object> baseLuisDialog;
        private readonly ILuisServiceProvider luisServiceProvider;


        public SkippableDateDialog(BaseLuisDialog<object> baseLuisDialog, ILuisServiceProvider luisServiceProvider, string promptMessage = null, string unrecognisedAnswerMessage = null)
            : base(promptMessage, unrecognisedAnswerMessage)
        {
            this.baseLuisDialog = baseLuisDialog;
            this.luisServiceProvider = luisServiceProvider;
        }

        protected override async Task HandleLuisResultAsync(IDialogContext context, LuisResult result)
        {
            if (result.TopScoringIntent.Intent != Intents.GetDate)
            {
                if (!await new SkippableDialogForwarder(this.baseLuisDialog, this.luisServiceProvider).TryToForward(context, result))
                {
                    await base.HandleLuisResultAsync(context, result);
                }
            }
            else
            {
                await base.HandleLuisResultAsync(context, result);
            }
        }
    }
}