namespace Objectivity.Bot.BaseDialogs.Tests
{
    using AutofacModules;
    using FluentAssertions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Utils;
    using Xunit;

    public class IntentPickerTests : IDisposable
    {
        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        private bool callbackValue;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async void Whether_IntentPicker_ExecutesCallback_On_DirectIntent()
        {
            this.callbackValue = false;
            var providerMock = new Mock<IIntentDescriptionProvider>();
            var loggerMock = new Mock<IIntentLogger>();
            var contextMock = new Mock<IDialogContext>();
            var appsettingsMock = new Mock<IApplicationSettings>();

            appsettingsMock.Setup(s => s.GetSetting(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string s1, string s2) => s2);

            var unit = new IntentsPicker(providerMock.Object, loggerMock.Object, appsettingsMock.Object);
            var result = new LuisResult
            {
                Intents = new List<IntentRecommendation>
                {
                    new IntentRecommendation("A", 0.55),
                    new IntentRecommendation("B", 0.44)
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
            var appsettingsMock = new Mock<IApplicationSettings>();
            appsettingsMock.Setup(s => s.GetSetting(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string s1, string s2) => s2);
            providerMock.Setup(provider => provider.GetDescriptions(It.IsAny<IEnumerable<string>>())).Returns(
                () => new List<IntentDescription>
                          {
                              new IntentDescription { Intent = "A", Description = "A" },
                              new IntentDescription { Intent = "B", Description = "B" }
                          });

            var unit = new IntentsPicker(providerMock.Object, loggerMock.Object, appsettingsMock.Object);
            var result = new LuisResult
            {
                Intents = new List<IntentRecommendation>
                {
                    new IntentRecommendation("A", 0.55),
                    new IntentRecommendation("B", 0.54)
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