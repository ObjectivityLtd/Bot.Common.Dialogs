namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    /// <summary>
    /// Allws to save values into bot state.
    /// </summary>
    public static class ContextHelper
    {
        /// <summary>
        /// Gets value from conversation context.
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="context">The dialog context.</param>
        /// <param name="key">Key.</param>
        /// <returns>Value for given key.</returns>
        public static T GetValueFromContext<T>(this IDialogContext context, string key)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return !context.UserData.TryGetValue(key, out T result)
                ? default(T)
                : result;
        }

        /// <summary>
        /// Gets value from user context.
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="context">The dialog context.</param>
        /// <param name="key">Key.</param>
        /// <returns>Value for given key.</returns>
        public static async Task<T> GetValueFromState<T>(this IDialogContext context, string key)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // TODO: error CS0618: 'StateClient' is obsolete: 'The StateAPI is being deprecated.  Please refer to https://aka.ms/yr235k for details on how to replace with your own storage.'
#pragma warning disable CS0618 // Type or member is obsolete
            using (StateClient stateClient = context.Activity.GetStateClient())
            {
                IBotState chatbotState = stateClient.BotState;
                BotData chatbotData =
                    await chatbotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);

                return chatbotData.GetProperty<T>(key);
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Sets value into conversation context.
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="context">The dialog context.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public static void SetValueIntoContext<T>(this IDialogContext context, string key, T value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.UserData.SetValue(key, value);
        }

        /// <summary>
        /// Sets value into user context.
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="context">The dialog context.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>Task.</returns>
        public static async Task SetValueIntoState<T>(this IDialogContext context, string key, T value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // TODO: error CS0618: 'StateClient' is obsolete: 'The StateAPI is being deprecated.  Please refer to https://aka.ms/yr235k for details on how to replace with your own storage.'
#pragma warning disable CS0618 // Type or member is obsolete

            using (StateClient stateClient = context.Activity.GetStateClient())
            {
                IBotState chatbotState = stateClient.BotState;
                BotData chatbotData =
                    await chatbotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);

                chatbotData.SetProperty(key, value);
                await chatbotState.SetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id, chatbotData);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        /// <summary>
        /// Gets value from conversation context.
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="context">The dialog context.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value retreived from context.</param>
        /// <returns>Value for given key.</returns>
        public static bool TryGetValueFromContext<T>(this IDialogContext context, string key, out T value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.UserData.TryGetValue(key, out value);
        }
    }
}