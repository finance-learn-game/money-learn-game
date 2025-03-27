using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.Infrastructure;

namespace SmbcApp.LearnGame.GamePlay.Domain
{
    /// <summary>
    ///     給料に関するドメインロジックを提供する
    /// </summary>
    internal sealed class SalaryDomain
    {
        private readonly AvatarRegistry _avatarRegistry;
        private readonly NetworkGameTurn _gameTurn;

        public SalaryDomain(AvatarRegistry avatarRegistry, NetworkGameTurn gameTurn)
        {
            _avatarRegistry = avatarRegistry;
            _gameTurn = gameTurn;
        }

        public void ApplySalary(PersistantPlayer player)
        {
            var avatarGuid = player.AvatarGuidState.AvatarGuid.Value.ToGuid();
            if (!_avatarRegistry.TryGetAvatar(avatarGuid, out var avatar)) return;

            var salary = avatar.SalaryConfig.CalculateSalary(_gameTurn.GameRange.Start, _gameTurn.CurrentTime);
            player.BalanceState.CurrentBalance += salary;
            player.BalanceState.GiveSalaryRpc(salary);
        }
    }
}