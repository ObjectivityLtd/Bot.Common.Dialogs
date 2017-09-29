namespace Objectivity.Bot.BaseDialogs.AutofacModules
{
    using System.Collections.Generic;

    public interface IIntentDescriptionProvider
    {
        IEnumerable<IntentDescription> GetDescriptions(string intent);

        IEnumerable<IntentDescription> GetDescriptions(IEnumerable<string> intents);
    }
}