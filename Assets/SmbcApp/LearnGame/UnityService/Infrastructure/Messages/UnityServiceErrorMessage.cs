using System;

namespace SmbcApp.LearnGame.UnityService.Infrastructure.Messages
{
    /// <summary>
    ///     UnityServiceのエラーメッセージ
    ///     主にUIで表示するために使用
    /// </summary>
    public readonly struct UnityServiceErrorMessage
    {
        public enum Service
        {
            Authentication,
            Session
        }

        public readonly string Title;
        public readonly string Message;
        public readonly Service AffectedService;
        public readonly Exception OriginalException;

        public UnityServiceErrorMessage(string title, string message, Service affectedService,
            Exception originalException)
        {
            Title = title;
            Message = message;
            AffectedService = affectedService;
            OriginalException = originalException;
        }
    }
}