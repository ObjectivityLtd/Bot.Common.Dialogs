namespace Objectivity.Bot.BaseDialogs.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;

    public class AppConfigLuisServiceProvider : ILuisServiceProvider
    {
        public IEnumerable<ILuisService> GetDefaultLuisServices(string cultureCode = "en-us")
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ILuisService> GetLuisServicesForDialog(Type dialogType, IDialogContext dialogContext)
        {
            bool isStaging;
            bool.TryParse(ConfigurationManager.AppSettings.Get("Staging"), out isStaging);
            return (ConfigurationManager.GetSection("LuisServices") as Hashtable)?.Cast<DictionaryEntry>()
                .ToDictionary(n => n.Key.ToString(), n => n.Value.ToString()).Select(
                    kvp => new LuisService(
                        new LuisModelAttribute(kvp.Key, kvp.Value)
                            {
                                Verbose = true,
                                Log = true,
                                SpellCheck = false,
                                Staging = isStaging
                            })).ToArray();
        }
    }
}