using ObservableCollections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers
{
    [CreateAssetMenu(
        fileName = "PersistantPlayerRuntimeCollection",
        menuName = "RuntimeDataContainers/PersistantPlayerRuntimeCollection")
    ]
    internal sealed class PersistantPlayerRuntimeCollection : ScriptableObject
    {
        [ReadOnly] [ShowInInspector] private readonly ObservableDictionary<ulong, PersistantPlayer> _players = new();

        public IReadOnlyObservableDictionary<ulong, PersistantPlayer> Players => _players;

        private void OnEnable()
        {
            _players.Clear();
        }

        public void AddPlayer(PersistantPlayer player)
        {
            _players.Add(player.OwnerClientId, player);
        }

        public void RemovePlayer(PersistantPlayer player)
        {
            _players.Remove(player.OwnerClientId);
        }

        public bool TryGetPlayer(ulong clientID, out PersistantPlayer player)
        {
            return _players.TryGetValue(clientID, out player);
        }
    }
}