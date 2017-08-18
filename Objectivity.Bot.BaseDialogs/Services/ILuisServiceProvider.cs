﻿namespace Bot.BaseDialogs.Services
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Bot.Builder.Luis;

    public interface ILuisServiceProvider
    {
        IEnumerable<ILuisService> GetLuisServicesForDialog(Type dialogType);
    }
}
