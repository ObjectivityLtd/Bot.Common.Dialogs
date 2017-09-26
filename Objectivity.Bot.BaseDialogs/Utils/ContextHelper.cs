namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    public static class ContextHelper
    {
        public static T GetValueFromContext<T>(this IDialogContext context, string key)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            T result;
            if (!context.UserData.TryGetValue(key, out result))
            {
                return default(T);
            }

            return result;
        }

        public static bool TryGetValueFromContext<T>(this IDialogContext context, string key, out T value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.UserData.TryGetValue(key, out value))
            {
                return true;
            }

            return false;
        }

        public static void SetValueIntoContext<T>(this IDialogContext context, string key, T value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.UserData.SetValue(key, value);
        }

        public static async Task SetValueIntoState<T>(this IDialogContext context, string key, T value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            using (StateClient stateClient = context.Activity.GetStateClient())
            {

                IBotState chatbotState = stateClient.BotState;
                BotData chatbotData = await chatbotState.GetUserDataAsync(
                    context.Activity.ChannelId, context.Activity.From.Id);

                chatbotData.SetProperty(key, value);
                await chatbotState.SetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id, chatbotData);
            }
        }

        public static async Task<T> GetValueFromState<T>(this IDialogContext context, string key)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            using (StateClient stateClient = context.Activity.GetStateClient())
            {
                IBotState chatbotState = stateClient.BotState;
                BotData chatbotData =
                    await chatbotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);

                return chatbotData.GetProperty<T>(key);
            }
        }

    }
}