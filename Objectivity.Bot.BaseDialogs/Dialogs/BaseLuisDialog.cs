namespace Objectivity.Bot.BaseDialogs.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;

    using Newtonsoft.Json;

    using NLog;

    using Objectivity.Bot.BaseDialogs.LuisApp;
    using Objectivity.Bot.BaseDialogs.Services;
    using Objectivity.Bot.BaseDialogs.Utils;

    [Serializable]
    public abstract class BaseLuisDialog<T> : IDialog<ILuisDialogResponse<T>>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public IDialogFactory DialogFactory { get; set; }

        public IIntentsPicker IntentsPicker { get; set; }

        public ILuisServiceProvider LuisServiceProvider { get; set; }

        public string UnrecognizedAnswerMessage { get; set; }

        public bool WaitForLuisResultOnStart { get; set; }

#pragma warning disable 1998
        public virtual async Task StartAsync(IDialogContext context)
#pragma warning restore 1998
        {
            if (this.WaitForLuisResultOnStart)
            {
                context.Wait<LuisResult>(this.LuisResultReceived);
            }
            else
            {
                context.Wait(this.MessageReceived);
            }
        }

        protected virtual IntentRecommendation BestIntentFrom(LuisResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            return result.TopScoringIntent ?? result.Intents?.MaxBy(i => i.Score ?? 0d);
        }

        protected virtual LuisServiceResult BestResultFrom(IEnumerable<LuisServiceResult> results)
        {
            return results.MaxBy(i => i.BestIntent.Score ?? 0d);
        }

        protected virtual async Task DefaultResumeAfterAsync<TResponse>(
            IDialogContext context,
            IAwaitable<ILuisDialogResponse<TResponse>> result)
        {
            var dialogResponse = await result;
            switch (dialogResponse.ResponseType)
            {
                case ResponseType.RedirectWithLuisResult:
                    this.Redirect(context, dialogResponse.LuisResult);
                    break;
                case ResponseType.RedirectWithIntent:
                    this.Redirect(context, dialogResponse.Intent);
                    break;
                default:
                    context.Wait(this.MessageReceived);
                    break;
            }
        }

        protected virtual void EndDialog(IDialogContext context, T response)
        {
            if (context == null)
            {
                var ex = new ArgumentNullException(nameof(context));
                Logger.Error(ex);
                throw ex;
            }

            context.Done(new LuisDialogResponse<T> { ResponseType = ResponseType.Regular, Response = response });
        }

        protected IEnumerable<KeyValuePair<string, IntentHandler>> EnumerateHandlers()
        {
            var type = this.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var method in methods)
            {
                var intents = method.GetCustomAttributes<LuisIntentAttribute>(inherit: true).ToArray();

                if (!intents.Any())
                {
                    continue;
                }

                IntentHandler intentHandler = null;
                try
                {
                    intentHandler = (IntentHandler)Delegate.CreateDelegate(
                        typeof(IntentHandler),
                        this,
                        method,
                        throwOnBindFailure: false);
                }
                catch (ArgumentException)
                {
                    Logger.Error(
                        $"Cannot bind to the target method {method.Name} because its signature or security transparency is not compatible with that of the delegate type.");
                }

                if (intentHandler != null)
                {
                    var intentNames = intents.Select(i => i.IntentName).DefaultIfEmpty(method.Name);

                    foreach (var intentName in intentNames)
                    {
                        yield return new KeyValuePair<string, IntentHandler>(intentName, intentHandler);
                    }
                }
                else
                {
                    if (intents.Length > 0)
                    {
                        throw new InvalidIntentHandlerException(
                            string.Join(";", intents.Select(i => i.IntentName)),
                            method);
                    }
                }
            }
        }

        protected virtual async Task ForwardLuisResultAsync<TBaseLuisDialog, TResponse>(
            IDialogContext context,
            LuisResult result,
            ResumeAfter<ILuisDialogResponse<TResponse>> resumeAfter = null)
            where TBaseLuisDialog : BaseLuisDialog<TResponse>
        {
            var dialog = this.DialogFactory.DialogFor<TBaseLuisDialog>();
            dialog.WaitForLuisResultOnStart = true;
            await context.Forward(dialog, resumeAfter ?? this.DefaultResumeAfterAsync, result, CancellationToken.None);
        }

        protected virtual async Task HandleLuisResultAsync(IDialogContext context, LuisResult result)
        {
            if (result?.Intents == null || !result.Intents.Any())
            {
                throw new ApplicationException("Luis result is empty");
            }

            await this.IntentsPicker.PickCorrectIntent(context, result, this.HandleLuisIntent);
        }

        protected virtual async Task LuisResultReceived(IDialogContext context, IAwaitable<LuisResult> result)
        {
            try
            {
                await this.HandleLuisResultAsync(context, await result);
            }
            catch (Exception e)
            {
                var activity = context.Activity as Activity;
                Logger.Fatal($"message: {activity?.Text} \n {e}");

                await context.PostAsync(Messages.CodeError);
                context.Wait(this.MessageReceived);
            }
        }

        protected virtual async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;
            var messageText = message.Text;

            // Modify request by the service to add attributes and then by the dialog to reflect the particular query
            var tasks = this.LuisServiceProvider.GetLuisServicesForDialog(this.GetType(), context).Select(
                service =>
                    {
                        var request = service.ModifyRequest(new LuisRequest(messageText));
                        return service.QueryAsync(request, context.CancellationToken);
                    }).ToArray();

            var results = await Task.WhenAll(tasks);

            var resultWithStrongestBestIntent = results
                .Select(result => new { Result = result, BestIntent = this.BestIntentFrom(result) })
                .Where(r => r.BestIntent != null).MaxBy(r => r.BestIntent.Score ?? 0d).Result;

            if (resultWithStrongestBestIntent == null)
            {
                throw new InvalidOperationException("No winning intent selected from Luis results.");
            }

            await this.HandleLuisResultAsync(context, resultWithStrongestBestIntent);
        }

        /// <summary>
        /// Parses dates to dd/mm/yyyy format in order to enchance LUIS parsing.
        /// </summary>
        /// <param name="result">Luis result</param>
        /// <param name="context">Dialog context</param>
        /// <param name="expectedIntent">Name of the luis intent responsible for resolving dates in LUIS</param>
        /// <returns>Luis result.</returns>
        protected async Task<LuisResult> ParseDatesAndResendIfNeeded(
            LuisResult result,
            IDialogContext context,
            string expectedIntent = Intents.GetDate)
        {
            if (DateParser.IsStringContainsDotsInDate(result.Query))
            {
                var parsedQuery = DateParser.ParseDotsToDashes(result.Query);
                bool.TryParse(ConfigurationManager.AppSettings.Get("Staging"), out bool isStaging);
                LuisRequest request = new LuisRequest(parsedQuery) { Staging = isStaging };
                List<LuisResult> results = new List<LuisResult>();
                foreach (var luisService in this.LuisServiceProvider.GetLuisServicesForDialog(this.GetType(), context))
                {
                    results.Add(await luisService.QueryAsync(request, CancellationToken.None));
                }

                return results.Where(s => s.TopScoringIntent.Intent == expectedIntent)
                    .OrderBy(s => s.TopScoringIntent.Score).FirstOrDefault();
            }

            return result;
        }

        protected virtual async Task PostAndWaitAsync(IDialogContext context, string message)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await context.PostAsync(message);
            context.Wait(this.MessageReceived);
        }

        protected async Task PostLuisResultAndWaitAsync(IDialogContext context, LuisResult result)
        {
            await this.PostAndWaitAsync(context, JsonConvert.SerializeObject(result));
        }

        /// <summary>
        /// Redirects flow to new another with new top scoring intent.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="intent">name of new LUIS intent.</param>
        protected virtual void Redirect(IDialogContext context, string intent)
        {
            if (context == null)
            {
                var ex = new ArgumentNullException(nameof(context));
                Logger.Error(ex);
                throw ex;
            }

            context.Done(new LuisDialogResponse<T> { ResponseType = ResponseType.RedirectWithIntent, Intent = intent });
        }

        /// <summary>
        /// Redirects flow to new another with new top scoring intent.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">Luis result.</param>
        protected virtual void Redirect(IDialogContext context, LuisResult result)
        {
            if (context == null)
            {
                var ex = new ArgumentNullException(nameof(context));
                Logger.Error(ex);
                throw ex;
            }

            context.Done(
                new LuisDialogResponse<T> { ResponseType = ResponseType.RedirectWithLuisResult, LuisResult = result });
        }

        protected bool RedirectIfNeeded<TResponse>(
            IDialogContext context,
            ILuisDialogResponse<TResponse> dialogResponse)
        {
            if (dialogResponse == null)
            {
                throw new ArgumentNullException(nameof(dialogResponse));
            }

            switch (dialogResponse.ResponseType)
            {
                case ResponseType.RedirectWithLuisResult:
                    this.Redirect(context, dialogResponse.LuisResult);
                    return true;
                case ResponseType.RedirectWithIntent:
                    this.Redirect(context, dialogResponse.Intent);
                    return true;
            }

            return false;
        }

        [LuisIntent("")]
        protected virtual async Task WildcardIntentHandlerAsync(IDialogContext context, LuisResult result)
        {
            var strongestIntent = result.GetStrongestIntent();
            var isKnownIntent = strongestIntent != null && strongestIntent.Intent != Intents.None;

            if (isKnownIntent)
            {
                this.Redirect(context, result);
            }
            else
            {
                await this.PostAndWaitAsync(context, this.UnrecognizedAnswerMessage ?? $"{Messages.DidNotUnderstand}");
            }
        }

        private async Task HandleLuisIntent(IDialogContext context, LuisResult result)
        {
            var knownHandlers = this.EnumerateHandlers().ToDictionary(kv => kv.Key, kv => kv.Value);
            if (!knownHandlers.TryGetValue(result.GetStrongestIntent().Intent, out IntentHandler handler))
            {
                handler = knownHandlers[string.Empty];
            }

            if (handler != null)
            {
                await handler(context, result);
            }
            else
            {
                var ex = new Exception("No default intent handler found.");
                Logger.Error(ex, ex.Message);
                throw ex;
            }
        }
    }
}