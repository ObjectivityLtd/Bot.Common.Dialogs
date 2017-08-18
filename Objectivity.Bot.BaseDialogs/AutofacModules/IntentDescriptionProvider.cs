namespace Objectivity.Bot.BaseDialogs.AutofacModules
{
    using System.Collections.Generic;
    using System.Linq;

    public class IntentDescriptionProvider : IIntentDescriptionProvider
    {
        private readonly IEnumerable<IntentDescription> intentDescriptions;

        public IntentDescriptionProvider(IEnumerable<IntentDescription> intentDescriptions)
        {
            this.intentDescriptions = intentDescriptions;
        }

        public IEnumerable<IntentDescription> GetDescriptions(string intent)
        {
            List<IntentDescription> descriptions = new List<IntentDescription>();
            var descriptionsForIntent = this.intentDescriptions.Where(s => s.Intent.Equals(intent)).ToList();
            if (descriptionsForIntent.Any())
            {
                descriptions.AddRange(descriptionsForIntent);
            }

            return descriptions;
        }

        public IEnumerable<IntentDescription> GetDescriptions(IEnumerable<string> intents)
        {
            List<IntentDescription> descriptions = new List<IntentDescription>();
            foreach (var intent in intents)
            {
                descriptions.AddRange(this.GetDescriptions(intent));
            }

            return descriptions;
        }
    }
}