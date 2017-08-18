namespace Objectivity.Bot.BaseDialogs.AutofacModules
{
    using System;

    [Serializable]
    public class IntentDescription
    {
        public string Intent { get; set; }
        public string Description { get; set; }
        public Type ResourceType { get; set; }
    }
}