namespace Objectivity.Bot.BaseDialogs.AutofacModules
{
    using System;

    [Serializable]
    public class IntentDescription
    {
        public string Description { get; set; }

        public string Intent { get; set; }

        public Type ResourceType { get; set; }
    }
}