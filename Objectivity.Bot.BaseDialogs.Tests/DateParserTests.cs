namespace Objectivity.Bot.BaseDialogs.Tests
{
    using FluentAssertions;
    using Utils;
    using Xunit;

    public class DateParserTests
    {
        [Fact]
        public void Whether_DateParser_ChangesDotsToDashes_On_ParseStringDate()
        {
            const string input = "from 20.04.2017 to 21.05.2017";
            const string expectedResult = "from 20-04-2017 to 21-05-2017";

            var result = DateParser.ParseDotsToDashes(input);

            result.Should().Be(expectedResult);
        }
    }
}