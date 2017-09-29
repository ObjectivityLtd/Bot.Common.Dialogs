namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System;
    using System.ComponentModel;
    using System.Resources;

    [AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Design",
        "CA1019:DefineAccessorsForAttributeArguments",
        Justification = "This is attribute class, there is no need for properties")]
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly ResourceManager resource;

        private readonly string resourceKey;

        public LocalizedDescriptionAttribute(string resourceKey, Type resourceType)
        {
            this.resource = new ResourceManager(resourceType);
            this.resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                string displayName = this.resource.GetString(this.resourceKey);

                return string.IsNullOrEmpty(displayName) ? $"[[{this.resourceKey}]]" : displayName;
            }
        }
    }
}