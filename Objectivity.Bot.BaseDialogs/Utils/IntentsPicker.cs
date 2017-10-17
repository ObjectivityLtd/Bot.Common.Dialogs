namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;

    using Objectivity.Bot.BaseDialogs.AutofacModules;
    using Objectivity.Bot.BaseDialogs.LuisApp;

    [Serializable]
    public class IntentsPicker : IIntentsPicker
    {
        private readonly IIntentDescriptionProvider intentDescriptionProvider;

        private readonly IIntentLogger intentLogger;

        private readonly IApplicationSettings applicationSettings;

        private double lowScoreThreshold;

        private int numberOfIntentsToConsider;

        private double scoreDifferenceThreshold;

        private Func<IDialogContext, LuisResult, Task> handlerCallback;

        private IEnumerable<IntentDescription> intentDescriptions;

        private string luisResultSerizalied;

        public IntentsPicker(IIntentDescriptionProvider intentDescriptionProvider, IIntentLogger intentLogger, IApplicationSettings applicationSettings)
        {
            this.intentDescriptionProvider = intentDescriptionProvider;
            this.intentLogger = intentLogger;
            this.applicationSettings = applicationSettings;
            this.GetThresholdsFromWebConfig();
        }

        public async Task PickCorrectIntent(
            IDialogContext context,
            LuisResult result,
            Func<IDialogContext, LuisResult, Task> callbackAction)
        {
            this.handlerCallback = callbackAction;

            if (result?.GetStrongestIntent().Score < this.lowScoreThreshold)
            {
                this.intentLogger.LogLuisResult(result, comment: "Low score");
                await this.handlerCallback(context, result.CopySettingNewIntent(Intents.None));
                return;
            }

            var intentsToCheck = result?.Intents.Take(this.numberOfIntentsToConsider).ToList();

            var differenceInScores = intentsToCheck?.Select(s => s.Score).Aggregate((i1, i2) => i1 - i2);
            if (intentsToCheck?.Count == this.numberOfIntentsToConsider
                && differenceInScores <= this.scoreDifferenceThreshold)
            {
                this.intentLogger.LogLuisResult(result, this.numberOfIntentsToConsider, "Simillar intents");

                this.intentDescriptions = this.intentDescriptionProvider
                    .GetDescriptions(intentsToCheck.Select(i => i.Intent)).ToList();

                if (this.intentDescriptions.Count() < intentsToCheck.Count)
                {
                    Trace.TraceError(string.Concat(Messages.Log_SimillarIntents, string.Join(", ", intentsToCheck)));

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
            var newResult = luisResult.CopySettingNewIntent(
                this.intentDescriptions.Single(s => s.Description.Equals(chosenDescription)).Intent);

            await this.handlerCallback(context, newResult);
        }

        private void GetThresholdsFromWebConfig()
        {
            this.numberOfIntentsToConsider = int.Parse(
                this.applicationSettings.GetSetting("NumberOfIntentsToPickForPrompt", "2"),
                CultureInfo.InvariantCulture);
            this.scoreDifferenceThreshold = double.Parse(
                this.applicationSettings.GetSetting("IntentScoreDifferenceThreshold", "0.15"),
                CultureInfo.InvariantCulture);
            this.lowScoreThreshold = double.Parse(
                this.applicationSettings.GetSetting("IntentLowScoreThreshold", "0.4"),
                CultureInfo.InvariantCulture);
        }
    }
}