namespace Objectivity.Bot.BaseDialogs.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;

    public interface ILuisServiceWrapper : ILuisService
    {
        Task<LuisResult> QueryAsync(LuisRequest request, CancellationToken token);
    }
}