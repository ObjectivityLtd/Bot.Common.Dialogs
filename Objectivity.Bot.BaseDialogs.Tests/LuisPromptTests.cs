namespace Objectivity.Bot.BaseDialogs.Tests
{
    using System.Threading.Tasks;
    using Bot.Tests.Stories.Recorder;
    using Bot.Tests.Stories.Xunit;
    using Xunit;

    public class LuisPromptTests : LuisDialogUnitTestBase<LuisPromptTestingDialog>
    {
        [Theory]
        [InlineData("Yes")]
        public async Task LuisDialog_PositiveResponse_YesReturned(string input)
        {
            var story = StoryRecorder
                .Record()
                .User.Says(input)
                .Bot.Says("What would you do now?")
                .Bot.Says("Returned: Yes")

                // dialog ends
                .Bot.Says("What would you do now?")
                .Rewind();
            await this.Play(story);
        }

        [Theory]
        [InlineData("No")]
        public async Task LuisDialog_NegativeResponse_NoReturned(string input)
        {
            var story = StoryRecorder
                .Record()
                .User.Says(input)
                .Bot.Says("What would you do now?")
                .Bot.Says("Returned: No")

                // dialog ends
                .Bot.Says("What would you do now?")
                .Rewind();
            await this.Play(story);
        }

        [Theory]
        [InlineData("Everything")]
        public async Task LuisDialog_KnownResponse_IntentReturned(string input)
        {
            var story = StoryRecorder
                .Record()
                .User.Says(input)
                .Bot.Says("What would you do now?")
                .Bot.Says("Returned intent: Everything")

                // dialog ends
                .Bot.Says("What would you do now?")
                .Rewind();
            await this.Play(story);
        }

        [Fact]
        public async Task LuisDialog_UnexpectedResponseOnce_AcceptsSecondResponse()
        {
            var story = StoryRecorder
                .Record()
                .User.Says("I don't know")
                .Bot.Says("What would you do now?")
                .Bot.Says("I didn't understand. Say something in reply.\r\nWhat would you do now?")
                .User.Says("Yes")
                .Bot.Says("Returned: Yes")

                // dialog ends
                .Bot.Says("What would you do now?")
                .Rewind();
            await this.Play(story);
        }

        [Fact]
        public async Task LuisDialog_UnexpectedResponseTwice_TooManyAttemptsReturned()
        {
            var story = StoryRecorder
                .Record()
                .User.Says("I don't know")
                .Bot.Says("What would you do now?")
                .Bot.Says("I didn't understand. Say something in reply.\r\nWhat would you do now?")
                .User.Says("I don't know")
                .Bot.Says("I have no idea what you meant")
                .Bot.Says("Returned: TooManyAttempts")

                // dialog ends
                .Bot.Says("What would you do now?")
                .Rewind();
            await this.Play(story);
        }
    }
}