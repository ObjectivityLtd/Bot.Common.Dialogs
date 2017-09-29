namespace Objectivity.Bot.BaseDialogs.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Bot.Builder.Luis;

    public class LuisModelProvider : ILuisModelProvider
    {
        private readonly IEnumerable<LuisServiceDialogRegistration> luisServiceDialogRegistrations;

        public LuisModelProvider(IEnumerable<LuisServiceDialogRegistration> luisServiceDialogRegistrations)
        {
            this.luisServiceDialogRegistrations = luisServiceDialogRegistrations;
        }

        public IEnumerable<ILuisModel> GetLuisModelsForDialog(Type dialogType)
        {
            if (this.luisServiceDialogRegistrations != null && this.luisServiceDialogRegistrations.Any())
            {
                return this.luisServiceDialogRegistrations.Where(r => r.DialogType == dialogType)
                    .Select(r => r.LuisModel).ToList();
            }

            return Enumerable.Empty<ILuisModel>();
        }
    }
}