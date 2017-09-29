namespace Objectivity.Bot.BaseDialogs.Tests
{
    using FluentAssertions;

    using Objectivity.Bot.BaseDialogs.Utils;

    using Xunit;

    public class DateParserTests
    {
        [Fact]
        public void Whether_DateParser_ChangesDotsToDashes_On_ParseStringDate()
        {
            var input = "from 20.04.2017 to 21.05.2017";
            var expectedResult = "from 20-04-2017 to 21-05-2017";

            var result = DateParser.ParseDotsToDashes(input);

            result.Should().Be(expectedResult);
        }
    }
}