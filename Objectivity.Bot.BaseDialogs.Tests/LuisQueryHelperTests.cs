namespace Objectivity.Bot.BaseDialogs.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Moq;
    using Services;
    using Xunit;

    public class LuisQueryHelperTests
    {
        [Fact]
        public void Given_ServicesCollectionIsNull_When_GetResultWithStrongestTopScoringIntentIsCalled_Then_NullReferenceExceptionIsThrown()
        {
            Func<Task> act = async () => await LuisQueryHelper.GetResultWithStrongestTopScoringIntent(null, "query", CancellationToken.None);

            act.ShouldThrow<NullReferenceException>("luisServices");
        }

        [Fact]
        public void Given_ServicesCollectionIsEmpty_When_GetResultWithStrongestTopScoringIntentIsCalled_Then_ArgumentExceptionIsThrown()
        {
            Func<Task> act = async () => await LuisQueryHelper.GetResultWithStrongestTopScoringIntent(Enumerable.Empty<ILuisServiceWrapper>(), "query", CancellationToken.None);

            act.ShouldThrow<ArgumentException>("luisServices");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Given_QueryTextIsNullOrWhitespace_When_GetResultWithStrongestTopScoringIntentIsCalled_Then_ArgumentExceptionIsThrown(string queryText)
        {
            Func<Task> act = async () =>
                await LuisQueryHelper.GetResultWithStrongestTopScoringIntent(new[] { new Mock<ILuisServiceWrapper>().Object }, queryText, CancellationToken.None);

            act.ShouldThrow<ArgumentException>("queryText");
        }

        [Theory]
        [InlineData("query")]
        public async Task Given_OneLuisServiceIsProvided_When_GetResultWithStrongestTopScoringIntentIsCalled_Then_ResultOfSingleServiceIsReturned(string queryText)
        {
            //Arrange
            var request = new LuisRequest(queryText);
            var token = CancellationToken.None;
            var luisServiceMock = new Mock<ILuisServiceWrapper>();
            luisServiceMock.Setup(m => m.ModifyRequest(It.Is<LuisRequest>(lr => lr.Query == queryText))).Returns(request);
            var luisResult = new LuisResult(queryText, new List<EntityRecommendation>(), new IntentRecommendation("IntentA", score: 0.95));
            luisServiceMock.Setup(m => m.QueryAsync(request, token)).ReturnsAsync(luisResult);

            //Act
            var result = await LuisQueryHelper.GetResultWithStrongestTopScoringIntent(new[] { luisServiceMock.Object }, queryText, token);

            //Assert
            result.ShouldBeEquivalentTo(luisResult);
        }

        [Theory]
        [InlineData("query")]
        public async Task Given_ManyLuisServices_When_GetResultWithStrongestTopScoringIntentIsCalled_Then_ResultWithStrongerTopScoringIntentIsReturned(string queryText)
        {
            //Arrange
            var request = new LuisRequest(queryText);
            var token = CancellationToken.None;

            var luisServiceMock1 = new Mock<ILuisServiceWrapper>();
            luisServiceMock1.Setup(m => m.ModifyRequest(It.Is<LuisRequest>(lr => lr.Query == queryText))).Returns(request);
            var luisResult = new LuisResult(queryText, new List<EntityRecommendation>(), new IntentRecommendation("IntentA", score: 0.95));
            luisServiceMock1.Setup(m => m.QueryAsync(request, token)).ReturnsAsync(luisResult);

            var luisServiceMock2 = new Mock<ILuisServiceWrapper>();
            luisServiceMock2.Setup(m => m.ModifyRequest(It.Is<LuisRequest>(lr => lr.Query == queryText))).Returns(request);
            var strongerLuisResult = new LuisResult(queryText, new List<EntityRecommendation>(), new IntentRecommendation("IntentB", score: 0.96));
            luisServiceMock2.Setup(m => m.QueryAsync(request, token)).ReturnsAsync(strongerLuisResult);

            //Act
            var result = await LuisQueryHelper.GetResultWithStrongestTopScoringIntent(new[] { luisServiceMock1.Object, luisServiceMock2.Object }, queryText, token);

            //Assert
            result.ShouldBeEquivalentTo(strongerLuisResult);
        }
    }
}
