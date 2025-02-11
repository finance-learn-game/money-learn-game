using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using SmbcApp.LearnGame.UnityService.Infrastructure.Messages;
using SmbcApp.LearnGame.Utils;
using Unity.Logging;
using Unity.Services.Authentication;
using Unity.Services.Core;
using VContainer;
using VContainer.Unity;

namespace SmbcApp.LearnGame.UnityService.Auth
{
    /// <summary>
    ///     認証サービスのファサード
    /// </summary>
    public sealed class AuthenticationServiceFacade : IAsyncStartable
    {
        private readonly IPublisher<UnityServiceErrorMessage> _errorMessagePublisher;
        private readonly ProfileManager _profileManager;

        [Inject]
        public AuthenticationServiceFacade(
            IPublisher<UnityServiceErrorMessage> errorMessagePublisher,
            ProfileManager profileManager
        )
        {
            _errorMessagePublisher = errorMessagePublisher;
            _profileManager = profileManager;
        }

        public bool IsInitialized { get; private set; }

        public async UniTask StartAsync(CancellationToken cancellation = new())
        {
            await UnityServices.InitializeAsync();
            IsInitialized = true;
        }

        public async UniTask SignIn(string profile = null)
        {
            try
            {
                profile ??= _profileManager.CurrentProfile.CurrentValue;
                if (string.IsNullOrEmpty(profile))
                {
                    Log.Warning("ProfileManager CurrentProfile is empty");
                    return;
                }

                var auth = AuthenticationService.Instance;
                if (auth.IsSignedIn) auth.SignOut();

                auth.SwitchProfile(profile);
                await auth.SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                PublishException(e);
                throw;
            }
        }

        private void PublishException(Exception err)
        {
            var reason = err.InnerException == null ? err.Message : $"{err.Message} ({err.InnerException.Message})";
            _errorMessagePublisher.Publish(new UnityServiceErrorMessage("Authentication Error", reason,
                UnityServiceErrorMessage.Service.Authentication, err));
        }
    }
}