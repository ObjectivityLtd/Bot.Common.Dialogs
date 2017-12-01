namespace Objectivity.Bot.BaseDialogs.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;

    [Serializable]
    public class LuisServiceStub : ILuisService
    {
        private readonly Dictionary<string, LuisResult> responses = new Dictionary<string, LuisResult>();
        private string query;

        public LuisServiceStub(Dictionary<string, IReadOnlyCollection<string>> intents)
        {
            if (intents == null)
            {
                throw new ArgumentNullException(nameof(intents));
            }

            foreach (var intent in intents)
            {
                foreach (var utterance in intent.Value)
                {
                    this.responses.Add(utterance, new LuisResult { TopScoringIntent = new IntentRecommendation(intent.Key) });
                }
            }
        }

        public LuisRequest ModifyRequest(LuisRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            this.query = request.Query;
            return request;
        }

        public Uri BuildUri(LuisRequest luisRequest)
        {
            return new Uri("http://luis.ai");
        }

        public Task<LuisResult> QueryAsync(Uri uri, CancellationToken token)
        {
            if (this.responses.TryGetValue(this.query, out LuisResult result))
            {
                return Task.FromResult(result);
            }

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Intent has not been set up for '{0}'", this.query));
        }
    }
}