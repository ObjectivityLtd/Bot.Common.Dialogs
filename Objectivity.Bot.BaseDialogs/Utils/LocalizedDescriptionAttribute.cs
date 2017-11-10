namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Resources;

    [AttributeUsage(AttributeTargets.All)]
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

                return string.IsNullOrEmpty(displayName) ? string.Format(CultureInfo.CurrentCulture, "[[{0}]]", this.resourceKey) : displayName;
            }
        }
    }
}