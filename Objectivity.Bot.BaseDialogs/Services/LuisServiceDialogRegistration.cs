namespace Bot.BaseDialogs.Services
{
    using System;
    using Microsoft.Bot.Builder.Luis;

    public class LuisServiceDialogRegistration
    {
        public Type DialogType { get; set; }
        public ILuisModel LuisModel { get; set; }
    }
}