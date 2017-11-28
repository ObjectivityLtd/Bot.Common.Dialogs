namespace Objectivity.Bot.BaseDialogs.Utils
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;

    public class BooleanRecognizer
    {
        private readonly string[][] patterns;

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanRecognizer"/> class.
        /// </summary>
        /// <param name="patterns">Yes and no alternatives for matching input where first dimension is either <see cref="PromptDialog.PromptConfirm.Yes"/> or <see cref="PromptDialog.PromptConfirm.No"/> and the arrays are alternative strings to match.</param>
        public BooleanRecognizer(string[][] patterns = null)
        {
            this.patterns = patterns ?? PromptDialog.PromptConfirm.Patterns;
        }

        public bool? RecognizeEntity(IMessageActivity messageActivity)
        {
            var yesKey = PromptDialog.PromptConfirm.Yes.ToString(CultureInfo.InvariantCulture);
            var noKey = PromptDialog.PromptConfirm.No.ToString(CultureInfo.InvariantCulture);
            var choices = new Dictionary<string, IReadOnlyList<string>>
            {
                {
                    yesKey,
                    this.patterns[PromptDialog.PromptConfirm.Yes].Select(x => x.ToLowerInvariant())
                        .ToList()
                },
                {
                    noKey,
                    this.patterns[PromptDialog.PromptConfirm.No].Select(x => x.ToLowerInvariant())
                        .ToList()
                }
            };

            var promptRecognizer = new PromptRecognizer();
            var entityMatches = promptRecognizer.RecognizeChoices(messageActivity, choices);
            var entityWinner = entityMatches.MaxBy(x => x.Score) ?? new RecognizeEntity<string>();
            return entityWinner.Entity == yesKey
                ? true
                : (entityWinner.Entity == noKey
                    ? (bool?)false
                    : null);
        }
    }
}