namespace Objectivity.Bot.BaseDialogs.Services
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Bot.Builder.Luis;

    public interface ILuisModelProvider
    {
        IEnumerable<ILuisModel> GetLuisModelsForDialog(Type dialogType);
    }
}