namespace Objectivity.Bot.BaseDialogs.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dialogs;
    using Microsoft.Bot.Builder.Dialogs;

    [Serializable]
    public class LuisPromptTestingDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            var intents = new Dictionary<string, IReadOnlyCollection<string>>
                {
                    { "None", new[] { "No", "Yes", "I don't know" } },
                    { "Everything", new[] { "Everything" } }
                };
            var luisService = new LuisServiceStub(intents);

            LuisPrompt.Confirm(luisService, context, Resume, "What would you do now?", 1, tooManyAttempts: "I have no idea what you meant");
            return Task.CompletedTask;
        }

        private static async Task Resume(IDialogContext context, IAwaitable<LuisPromptResult> result)
        {
            var response = await result;
            if (response.ResultType == LuisPromptResultType.LuisResult)
            {
                await context.PostAsync($"Returned intent: {response.LuisResult.TopScoringIntent.Intent}");
            }
            else
            {
                await context.PostAsync($"Returned: {response.ResultType}");
            }
            context.Done<object>(null);
        }
    }
}
