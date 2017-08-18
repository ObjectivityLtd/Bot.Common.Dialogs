namespace Bot.BaseDialogs.Utils
{
    public interface IApplicationSettings
    {
        string Get(string key);

        string Get(string key, string defaultValue);

        string this[string key] { get; }
    }
}