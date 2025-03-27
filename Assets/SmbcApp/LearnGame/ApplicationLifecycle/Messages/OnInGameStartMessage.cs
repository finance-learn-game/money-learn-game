using System;

namespace SmbcApp.LearnGame.ApplicationLifecycle.Messages
{
    public readonly struct OnInGameStartMessage
    {
        public readonly DateTime StartDate;
        public readonly DateTime EndDate;

        public OnInGameStartMessage(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}