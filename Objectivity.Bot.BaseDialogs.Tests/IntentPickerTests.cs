namespace Objectivity.Bot.BaseDialogs.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;

    using Moq;

    using Objectivity.Bot.BaseDialogs.AutofacModules;
    using Objectivity.Bot.BaseDialogs.Utils;

    using Xunit;

    public class IntentPickerTests : IDisposable
    {
        AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        private bool callbackValue;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async void Whether_IntentPicker_ExecutesCallback_On_DirectIntent()
        {
            this.callbackValue = false;
            var providerMock = new Mock<IIntentDescriptionProvider>();
            var loggerMock = new Mock<IIntentLogger>();
            var contextMock = new Mock<IDialogContext>();

            var unit = new IntentsPicker(providerMock.Object, loggerMock.Object);
            var result = new LuisResult()
                             {
                                 Intents =
                                     new List<IntentRecommendation>()
                                         {
                                             new IntentRecommendation(
                                                 "A",
                                                 0.55),
                                             new IntentRecommendation(
                                                 "B",
                                                 0.44)
                                         }
                             };

            await unit.PickCorrectIntent(contextMock.Object, result, this.CallbackAction);
            this.autoResetEvent.WaitOne(500);

            this.callbackValue.Should().Be(true);
        }

        [Fact]
        public async Task Whether_IntentPicker_ShowsPrompt_On_2SimilarIntents()
        {
            // arrange
            var providerMock = new Mock<IIntentDescriptionProvider>();
            var loggerMock = new Mock<IIntentLogger>();
            var contextMock = new Mock<IDialogContext>();

            providerMock.Setup(provider => provider.GetDescriptions(It.IsAny<IEnumerable<string>>())).Returns(
                () => new List<IntentDescription>()
                          {
                              new IntentDescription() { Intent = "A", Description = "A" },
                              new IntentDescription() { Intent = "B", Description = "B" }
                          });

            var unit = new IntentsPicker(providerMock.Object, loggerMock.Object);
            var result = new LuisResult()
                             {
                                 Intents =
                                     new List<IntentRecommendation>()
                                         {
                                             new IntentRecommendation(
                                                 "A",
                                                 0.55),
                                             new IntentRecommendation(
                                                 "B",
                                                 0.54)
                                         }
                             };

            // act
            await unit.PickCorrectIntent(contextMock.Object, result, this.CallbackAction);

            // assert
            providerMock.Verify(
                provider => provider.GetDescriptions(It.IsAny<IEnumerable<string>>()),
                Times.Exactly(1));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.autoResetEvent.Dispose();
            }
        }

        private Task CallbackAction(IDialogContext dialogContext, LuisResult luisResult)
        {
            this.callbackValue = true;
            this.autoResetEvent.Set();
            return Task.CompletedTask;
        }
    }
}