namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System.Configuration;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;

    using Objectivity.Bot.BaseDialogs.Dialogs;
    using Objectivity.Bot.BaseDialogs.LuisApp;
    using Objectivity.Bot.BaseDialogs.Services;

    public class SkippableDialogForwarder
    {
        private readonly BaseLuisDialog<object> baseLuisDialog;

        private readonly ILuisServiceProvider luisServiceProvider;

        public SkippableDialogForwarder(BaseLuisDialog<object> baseLuisDialog, ILuisServiceProvider luisServiceProvider)
        {
            this.baseLuisDialog = baseLuisDialog;
            this.luisServiceProvider = luisServiceProvider;
        }

        private static double MinimalReasonableIntentScroing => double.Parse(
            ConfigurationManager.AppSettings.Get("SkippableIntentScoringTreshold"));

        public async Task<bool> TryToForward(IDialogContext context, IMessageActivity messageActivity)
        {
            var messageText = messageActivity.Text;
            var tasks = this.GetLuisIntents(context, messageText);

            LuisResult[] luisResults = await Task.WhenAll(tasks);
            return await this.TryToForward(context, luisResults);
        }

        public async Task<bool> TryToForward(IDialogContext context, LuisResult luisResult)
        {
            return await this.TryToForward(context, new[] { luisResult });
        }

        private Task<LuisResult>[] GetLuisIntents(IDialogContext context, string messageText)
        {
            return this.luisServiceProvider.GetLuisServicesForDialog(this.GetType(), context).Select(
                service =>
                    {
                        var request = service.ModifyRequest(new LuisRequest(messageText));
                        return service.QueryAsync(request, context.CancellationToken);
                    }).ToArray();
        }

        private async Task<bool> TryToForward(IDialogContext context, LuisResult[] luisResults)
        {
            LuisResult luisResult = luisResults?.FirstOrDefault(
                lr => lr != null && lr.TopScoringIntent.Score > MinimalReasonableIntentScroing
                      && lr.TopScoringIntent.Intent != Intents.None);
            if (luisResult == null)
            {
                return false;
            }

            var messageActivity = context.MakeMessage();
            messageActivity.Text = luisResult.Query;
            await Task.Run(() => context.Reset(), CancellationToken.None);
            await context.Forward(this.baseLuisDialog, null, messageActivity, CancellationToken.None);

            return true;
        }
    }
}