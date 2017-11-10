﻿namespace Objectivity.Bot.BaseDialogs.Dialogs
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using LuisApp;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;

    [Serializable]
    public class DateDialog : PromptValueDialog<DateTime?>
    {
        private const string ParameterName = "date";

        public DateDialog(string promptMessage = null, string unrecognisedAnswerMessage = null)
        {
            var specifyRequest = string.Format(CultureInfo.InvariantCulture, Messages.SpecifyParameter, ParameterName);
            this.PromptMessage = promptMessage ?? specifyRequest;
            this.UnrecognizedAnswerMessage = unrecognisedAnswerMessage
                ?? string.Format(CultureInfo.InvariantCulture, "{0} {1}", Messages.DidNotUnderstand, specifyRequest);
        }

        [LuisIntent(Intents.GetDate)]
        public async Task GetDate(IDialogContext context, LuisResult result)
        {
            result = await this.ParseDatesAndResendIfNeeded(result, context);
            await this.HandleIntent(context, result);
        }

        protected override DateTime? GetValue(IDialogContext context, LuisResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            return result.Entities.FirstOrDefault(e => e.Type == EntityTypes.Datev2)?.ToDate();
        }
    }
}