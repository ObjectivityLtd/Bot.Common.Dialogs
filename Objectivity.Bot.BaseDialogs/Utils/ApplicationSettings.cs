namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System.Configuration;

    public class ApplicationSettings : IApplicationSettings
    {
        public string this[string key] => this.GetSetting(key);

        public string GetSetting(string key) => ConfigurationManager.AppSettings[key];

        public string GetSetting(string key, string defaultValue) => this.GetSetting(key) ?? defaultValue;
    }
}