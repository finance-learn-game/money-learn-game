using UnityEngine;

namespace SmbcApp.LearnGame.UnityService.Infrastructure
{
    internal sealed class RateLimitCooldown
    {
        private readonly float _cooldownTimeLength;
        private float _coolDownFinishedTime;

        public RateLimitCooldown(float cooldownTimeLength)
        {
            _cooldownTimeLength = cooldownTimeLength;
            _coolDownFinishedTime = -1f;
        }

        public bool Call()
        {
            if (Time.unscaledTime < _coolDownFinishedTime) return false;

            _coolDownFinishedTime = Time.unscaledTime + _cooldownTimeLength;
            return true;
        }
    }
}