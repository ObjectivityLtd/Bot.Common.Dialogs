namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System;

    public static class ExceptionExtensions
    {
        public static void TraceError(this Exception exception)
        {
            System.Diagnostics.Trace.TraceError($"{exception}");
        }
    }
}
