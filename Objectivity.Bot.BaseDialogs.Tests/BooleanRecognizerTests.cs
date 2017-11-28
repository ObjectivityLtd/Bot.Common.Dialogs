namespace Objectivity.Bot.BaseDialogs.Tests
{
    using Microsoft.Bot.Builder.Resource;
    using Microsoft.Bot.Connector;
    using Moq;
    using Utils;
    using Xunit;

    public class BooleanRecognizerTests
    {
        [Theory]
        [InlineData("Yes")]
        [InlineData("Yes, thank you")]
        [InlineData("Yes, please")]
        [InlineData("Yes, thanks")]
        public void BooleanRecognizer_PositiveExpression_ReturnsTrue(string input)
        {
            var booleanRecognizer = new BooleanRecognizer();
            var msg = new Mock<IMessageActivity>();
            msg.SetupGet(m => m.Text).Returns(input);

            var result = booleanRecognizer.RecognizeEntity(msg.Object);

            Assert.True(result);
        }

        [Theory]
        [InlineData("No")]
        [InlineData("No, thank you")]
        [InlineData("no, thanks")]
        [InlineData("no, please")]
        [InlineData("nope")]
        public void BooleanRecognizer_NegativeExpression_ReturnsFalse(string input)
        {
            var booleanRecognizer = new BooleanRecognizer();
            var msg = new Mock<IMessageActivity>();
            msg.SetupGet(m => m.Text).Returns(input);

            var result = booleanRecognizer.RecognizeEntity(msg.Object);

            Assert.False(result);
        }

        [Theory]
        [InlineData("Maybe")]
        [InlineData("I don't know")]
        public void BooleanRecognizer_NeutralExpression_ReturnsNull(string input)
        {
            var booleanRecognizer = new BooleanRecognizer();
            var msg = new Mock<IMessageActivity>();
            msg.SetupGet(m => m.Text).Returns(input);

            var result = booleanRecognizer.RecognizeEntity(msg.Object);

            Assert.Null(result);
        }

        [Theory]
        [InlineData("nop")]
        [InlineData("nop, thanks")]
        [InlineData("nope")]
        [InlineData("no")]
        public void BooleanRecognizerWithCustomPatterns_MisspelledNegativeExpression_ReturnsFalse(string input)
        {
            string[][] patterns = { "Yes;y;sure;ok;yep".SplitList(), "No;n;nope;Nop".SplitList() };

            var booleanRecognizer = new BooleanRecognizer(patterns);
            var msg = new Mock<IMessageActivity>();
            msg.SetupGet(m => m.Text).Returns(input);

            var result = booleanRecognizer.RecognizeEntity(msg.Object);

            Assert.False(result);
        }
    }
}