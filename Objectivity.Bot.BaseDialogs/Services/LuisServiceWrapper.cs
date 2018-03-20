namespace Objectivity.Bot.BaseDialogs.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;

    public class LuisServiceWrapper : ILuisServiceWrapper
    {
        private readonly ILuisService luisService;

        public LuisServiceWrapper(ILuisService luisService)
        {
            this.luisService = luisService;
        }

        public LuisRequest ModifyRequest(LuisRequest request)
        {
            return this.luisService.ModifyRequest(request);
        }

        public Uri BuildUri(LuisRequest luisRequest)
        {
            return this.luisService.BuildUri(luisRequest);
        }

        public Task<LuisResult> QueryAsync(Uri uri, CancellationToken token)
        {
            return this.luisService.QueryAsync(uri, token);
        }

        public Task<LuisResult> QueryAsync(LuisRequest request, CancellationToken token)
        {
            return this.luisService.QueryAsync(request, token);
        }
    }
}