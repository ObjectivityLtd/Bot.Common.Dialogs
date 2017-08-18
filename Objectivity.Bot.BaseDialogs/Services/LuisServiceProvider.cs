namespace Objectivity.Bot.BaseDialogs.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;

    internal class LuisServiceProvider : ILuisServiceProvider
    {
        private readonly ILuisModelProvider luisModelProvider;

        public LuisServiceProvider(ILuisModelProvider luisModelProvider)
        {
            this.luisModelProvider = luisModelProvider;
        }

        public IEnumerable<ILuisService> GetLuisServicesForDialog(Type dialogType)
        {
            if (dialogType == null)
            {
                throw new ArgumentNullException(nameof(dialogType));
            }

            if (! dialogType.IsAssignableFrom(typeof(IDialog)))
            {
                throw new ArgumentException($"{dialogType.Name} type does not implement IDialog");
            }

            var luisModels = this.luisModelProvider.GetLuisModelsForDialog(dialogType);
            if (luisModels != null && luisModels.Any())
            {
                return luisModels.Select(m => new LuisService(m)).ToList();
            }

            return Enumerable.Empty<ILuisService>();
        }
    }
}
