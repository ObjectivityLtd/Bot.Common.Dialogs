namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using AutofacModules;
    using LuisApp;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;
    using NLog;

    [Serializable]
    public class IntentsPicker : IIntentsPicker
    {
        private readonly int numberOfIntentsToConsider;
        private readonly double scoreDifferenceThreshold;
        private readonly double lowScoreThreshold;

        private IEnumerable<IntentDescription> intentDescriptions;
        private readonly IIntentDescriptionProvider intentDescriptionProvider;
        private readonly IIntentLogger intentLogger;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private string luisResultSerizalied;
        private Func<IDialogContext, LuisResult, Task> handlerCallback;

        public IntentsPicker(IIntentDescriptionProvider intentDescriptionProvider, IIntentLogger intentLogger)
        {
            this.intentDescriptionProvider = intentDescriptionProvider;
            this.intentLogger = intentLogger;
            this.numberOfIntentsToConsider =
                Int32.Parse(ConfigurationManager.AppSettings["NumberOfIntentsToPickForPrompt"], CultureInfo.InvariantCulture);
            this.scoreDifferenceThreshold =
                double.Parse(ConfigurationManager.AppSettings["IntentScoreDifferenceThreshold"], CultureInfo.InvariantCulture);
            this.lowScoreThreshold = double.Parse(ConfigurationManager.AppSettings["IntentLowScoreThreshold"], CultureInfo.InvariantCulture);
        }

        public async Task PickCorrectIntent(IDialogContext context, LuisResult result, Func<IDialogContext, LuisResult, Task> callbackAction)
        {
            this.handlerCallback = callbackAction;

            if (result?.GetStrongestIntent().Score < this.lowScoreThreshold)
            {
                intentLogger.LogLuisResult(result, comment: "Low score");
                await this.handlerCallback(context, result.CopySettingNewIntent(Intents.None));
                return;
            }

            var intentsToCheck = result?.Intents.Take(this.numberOfIntentsToConsider).ToList();

            var differenceInScores = intentsToCheck?.Select(s => s.Score).Aggregate((i1, i2) => i1 - i2);
            if (intentsToCheck?.Count == this.numberOfIntentsToConsider && differenceInScores <= this.scoreDifferenceThreshold)
            {
                this.intentLogger.LogLuisResult(result, this.numberOfIntentsToConsider, "Simillar intents");

                this.intentDescriptions = this.intentDescriptionProvider
                    .GetDescriptions(intentsToCheck.Select(i => i.Intent)).ToList();

                if (this.intentDescriptions.Count() < intentsToCheck.Count)
                {
                    Logger.Fatal(string.Concat(Messages.Log_SimillarIntents,
                        string.Join(", ", intentsToCheck)));

                    await this.handlerCallback(context, result);
                    return;
                }

                this.luisResultSerizalied = result.ToJson();

                PromptDialog.Choice(
                    context,
                    this.ResumeAfterPickingIntent,
                    this.intentDescriptions.Select(s => s.Description),
                    Messages.SimillarIntents);
            }
            else
            {
                await this.handlerCallback(context, result);
            }
        }

        private async Task ResumeAfterPickingIntent(IDialogContext context, IAwaitable<object> result)
        {
            var chosenDescription = await result;
            var luisResult = this.luisResultSerizalied.ToLuisResult();
            var newResult = luisResult.CopySettingNewIntent(this.intentDescriptions
                .Single(s => s.Description.Equals(chosenDescription)).Intent);

            await this.handlerCallback(context, newResult);
        }
    }
}