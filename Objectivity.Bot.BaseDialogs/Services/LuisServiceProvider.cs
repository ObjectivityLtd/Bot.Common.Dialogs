namespace Objectivity.Bot.BaseDialogs.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;

    public class LuisServiceProvider : ILuisServiceProvider
    {
        private readonly ILuisModelProvider luisModelProvider;

        public LuisServiceProvider(ILuisModelProvider luisModelProvider)
        {
            this.luisModelProvider = luisModelProvider;
        }

        public IEnumerable<ILuisService> GetDefaultLuisServices(string cultureCode = "en-us")
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ILuisService> GetLuisServicesForDialog(Type dialogType, IDialogContext dialogContext)
        {
            if (dialogType == null)
            {
                throw new ArgumentNullException(nameof(dialogType));
            }

            if (!dialogType.IsAssignableFrom(typeof(IDialog)))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} type does not implement IDialog", dialogType.Name));
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