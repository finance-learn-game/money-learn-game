using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.UnityService.Infrastructure;
using SmbcApp.LearnGame.UnityService.Infrastructure.Messages;
using SmbcApp.LearnGame.Utils;
using Unity.Logging;
using Unity.Services.Multiplayer;
using VContainer;

namespace SmbcApp.LearnGame.UnityService.Session
{
    /// <summary>
    ///     セッションサービスのファサード
    /// </summary>
    public sealed class SessionServiceFacade
    {
        private readonly IPublisher<UnityServiceErrorMessage> _errMsgPublisher;
        private readonly ProfileManager _profileManager;
        private readonly RateLimitCooldown _rateLimitJoin;
        private readonly RateLimitCooldown _rateLimitServer;

        [Inject]
        public SessionServiceFacade(
            IPublisher<UnityServiceErrorMessage> errMsgPublisher,
            ProfileManager profileManager
        )
        {
            _errMsgPublisher = errMsgPublisher;
            _profileManager = profileManager;
            _rateLimitJoin = new RateLimitCooldown(3f);
            _rateLimitServer = new RateLimitCooldown(3f);
        }

        public ISession CurrentSession { get; private set; }

        public async UniTask<bool> TryCreateSession()
        {
            if (!_rateLimitServer.Call())
            {
                Log.Warning("Rate limit exceeded for creating a session.");
                return false;
            }

            try
            {
                CurrentSession = await MultiplayerService.Instance.CreateSessionAsync(new SessionOptions
                {
                    MaxPlayers = GameConfiguration.Instance.MaxPlayers,
                    IsPrivate = true
                });
                return true;
            }
            catch (Exception e)
            {
                PublishException(e);
            }

            return false;
        }

        public async UniTask<bool> TryJoinSession(string sessionCode)
        {
            if (string.IsNullOrEmpty(sessionCode)) return false;

            if (!_rateLimitJoin.Call())
            {
                Log.Warning("Rate limit exceeded for joining a session.");
                return false;
            }

            try
            {
                var option = new JoinSessionOptions
                {
                    PlayerProperties = CreatePlayerProperty()
                };
                CurrentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode, option);
                return true;
            }
            catch (Exception e)
            {
                PublishException(e);
            }

            return false;
        }

        public async UniTask LeaveSessionAsync()
        {
            if (CurrentSession == null) return;

            try
            {
                await CurrentSession.LeaveAsync();
                CurrentSession = null;
            }
            catch (Exception e)
            {
                PublishException(e);
            }
        }

        public async UniTask RemovePlayerFromSession(string uasId)
        {
            if (CurrentSession == null) return;
            if (!CurrentSession.IsHost) return;

            try
            {
                await CurrentSession.AsHost().RemovePlayerAsync(uasId);
            }
            catch (Exception e)
            {
                PublishException(e);
            }
        }

        private Dictionary<string, PlayerProperty> CreatePlayerProperty()
        {
            var profile = _profileManager.CurrentProfile.CurrentValue;
            if (!string.IsNullOrEmpty(profile))
                return new Dictionary<string, PlayerProperty>
                {
                    { "DisplayName", new PlayerProperty(profile, VisibilityPropertyOptions.Member) }
                };

            Log.Warning("ProfileManager CurrentProfile is empty");
            return null;
        }

        public bool TryGetDisplayName(IReadOnlyPlayer player, out string displayName)
        {
            if (player.Properties.TryGetValue("DisplayName", out var property))
            {
                displayName = property.Value;
                return true;
            }

            displayName = null;
            return false;
        }

        private void PublishException(Exception err)
        {
            var reason = err.InnerException == null ? err.Message : $"{err.Message} ({err.InnerException.Message})";
            _errMsgPublisher.Publish(new UnityServiceErrorMessage("Session Error", reason,
                UnityServiceErrorMessage.Service.Session, err));
        }
    }
}