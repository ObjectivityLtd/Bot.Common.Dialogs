namespace Bot.BaseDialogs.Utils
{
    using System.Configuration;

    public class ApplicationSettings : IApplicationSettings
    {
        public string Get(string key) => ConfigurationManager.AppSettings[key];

        public string Get(string key, string defaultValue) => this.Get(key) ?? defaultValue;

        public string this[string key] => this.Get(key);
    }
}