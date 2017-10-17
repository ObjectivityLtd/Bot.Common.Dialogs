namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System;
    using System.Diagnostics;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Connector;

    /// <summary>
    /// This is a copy of a class from Bot.Builder project which is sealed. When they will change it we should inherit from their implementation.
    /// </summary>
    public sealed class CustomPostUnhandledExceptionToUser : IPostToBot
    {
        private readonly IBotToUser botToUser;

        private readonly IPostToBot inner;

        private readonly TraceListener trace;

        public CustomPostUnhandledExceptionToUser(IPostToBot inner, IBotToUser botToUser, TraceListener trace)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.botToUser, nameof(botToUser), botToUser);
            SetField.NotNull(out this.trace, nameof(trace), trace);
        }

        async Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            try
            {
                await this.inner.PostAsync(activity, token);
            }
            catch (Exception error)
            {
                try
                {
                    if (Debugger.IsAttached)
                    {
                        var message = this.botToUser.MakeMessage();
                        message.Text = $"Exception: {error.Message}";
                        message.Attachments =
                            new[] { new Attachment(MediaTypeNames.Text.Plain, content: error.StackTrace) };

                        await this.botToUser.PostAsync(message, token);
                    }
                    else
                    {
                        await this.botToUser.PostAsync(Messages.CodeError, cancellationToken: token);
                    }

                    Trace.TraceError("message: " + ((Activity)activity).Text + "\n" + error);
                }
                catch (Exception inner)
                {
                    inner.TraceError();
                }

                throw;
            }
        }
    }
}