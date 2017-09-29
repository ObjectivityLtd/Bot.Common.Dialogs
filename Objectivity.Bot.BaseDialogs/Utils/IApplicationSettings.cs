namespace Objectivity.Bot.BaseDialogs.Utils
{
    public interface IApplicationSettings
    {
        string this[string key] { get; }

        string GetSetting(string key);

        string GetSetting(string key, string defaultValue);
    }
}