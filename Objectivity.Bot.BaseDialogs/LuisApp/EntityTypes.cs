namespace Bot.BaseDialogs.LuisApp
{
    public static class EntityTypes
    {
        public const string ClientEntityType = "Client";
        public const string ClientRefEntityType = "ClientRef";
        public const string DurationEntityType = "builtin.datetime.duration";
        public const string Subject = "Subject";
        public const string UserNameEntityType = "UserName";
        public const string BusTimetableSubject = nameof(EntityTypes.BusTimetableSubject);
        public const string Destination = nameof(EntityTypes.Destination);
        public const string Person = nameof(EntityTypes.Person);
        public const string Resource = nameof(EntityTypes.Resource);
        public const string Room = nameof(EntityTypes.Room);
        public const string TeamLead = nameof(EntityTypes.TeamLead);
        public const string PM = nameof(EntityTypes.PM);
        public const string HolidayType = nameof(EntityTypes.HolidayType);
        public const string Number = "builtin.number";
        public const string Datev2 = "builtin.datetimeV2.date";
        public const string Date = "builtin.datetime.date";
        public const string DateRange = "builtin.datetimeV2.daterange";
    }
}