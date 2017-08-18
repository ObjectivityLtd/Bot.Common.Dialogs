namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System;
    using Microsoft.Bot.Builder.Dialogs;

    public static class ContextHelper
    {
        public static T GetValueFromContext<T>(IDialogContext context, string key)
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

        public static bool TryGetValueFromContext<T>(IDialogContext context, string key, out T value)
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

        public static void SetValueIntoContext<T>(IDialogContext context, string key, T value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.UserData.SetValue(key, value);
        }
    }
}