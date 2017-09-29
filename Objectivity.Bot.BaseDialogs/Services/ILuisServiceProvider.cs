﻿namespace Objectivity.Bot.BaseDialogs.Services
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;

    public interface ILuisServiceProvider
    {
        IEnumerable<ILuisService> GetDefaultLuisServices(string cultureCode = "en-us");

        IEnumerable<ILuisService> GetLuisServicesForDialog(Type dialogType, IDialogContext dialogContext);
    }
}