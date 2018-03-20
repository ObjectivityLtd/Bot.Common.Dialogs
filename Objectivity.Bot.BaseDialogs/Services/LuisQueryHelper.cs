namespace Objectivity.Bot.BaseDialogs.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using NLog;

    public static class LuisQueryHelper
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets luis response with the strongest top scoring intent
        /// </summary>
        public static async Task<LuisResult> GetResultWithStrongestTopScoringIntent(
            IEnumerable<ILuisServiceWrapper> luisServices, string queryText, CancellationToken cancellationToken)
        {
            if (luisServices == null)
            {
                throw new NullReferenceException(nameof(luisServices));
            }

            if (!luisServices.Any())
            {
                throw new ArgumentException(nameof(luisServices));
            }

            if (string.IsNullOrWhiteSpace(queryText))
            {
                throw new ArgumentException(nameof(queryText));
            }

            var results = new List<LuisResult>();

            foreach (var service in luisServices)
            {
                results.Add(await QueryLuis(service, queryText, cancellationToken));
            }

            return results.OrderByDescending(r => r.TopScoringIntent.Score).FirstOrDefault();
        }

        public static IList<ILuisServiceWrapper> ToLuisServiceWrappers(this IEnumerable<ILuisService> luisServices)
        {
            return luisServices.Select(s => new LuisServiceWrapper(s)).Cast<ILuisServiceWrapper>().ToList();
        }

        private static async Task<LuisResult> QueryLuis(ILuisServiceWrapper luisService, string input, CancellationToken cancellationToken)
        {
            Logger.Trace(CultureInfo.InvariantCulture, "Querying LUIS for {0}", input);

            var request = luisService.ModifyRequest(new LuisRequest(input));
            var luisResult = await luisService.QueryAsync(request, cancellationToken);
            return luisResult;
        }
    }
}