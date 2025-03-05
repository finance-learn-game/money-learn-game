using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.Infrastructure;
using SmbcApp.LearnGame.UIWidgets.ScrollView;
using Unity.Logging;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using EventType =
    Unity.Netcode.NetworkListEvent<
        SmbcApp.LearnGame.GamePlay.GameState.NetworkData.NetworkAvatarSelection.SessionPlayerState>.EventType;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class PlayerListView : MonoBehaviour
    {
        [SerializeField] [Required] private UIScrollView scrollView;
        [SerializeField] [Required] private PlayerListItemView.Ref playerListItemViewPrefab;
        [SerializeField] [Required] private AvatarRegistry avatarRegistry;

        private readonly Dictionary<ulong, PlayerListItemView> _playerListItems = new();

        [Inject] internal NetworkAvatarSelection NetworkAvatarSelection;

        private void Start()
        {
            // 初期化
            var players = NetworkAvatarSelection.SessionPlayers;
            foreach (var player in players) AddPlayerToListView(player).Forget();

            players.OnListChanged += OnChangePlayers;
        }

        private void OnDestroy()
        {
            if (NetworkAvatarSelection?.SessionPlayers != null)
                NetworkAvatarSelection.SessionPlayers.OnListChanged -= OnChangePlayers;
        }

        private void OnChangePlayers(NetworkListEvent<NetworkAvatarSelection.SessionPlayerState> ev)
        {
            switch (ev.Type)
            {
                case EventType.Add:
                    AddPlayerToListView(ev.Value).Forget();
                    break;
                case EventType.Remove:
                    RemovePlayerFromListView(ev.Value.ClientId);
                    break;
                case EventType.Value:
                {
                    if (_playerListItems.TryGetValue(ev.Value.ClientId, out var playerListItem))
                    {
                        playerListItem.PlayerName = ev.Value.PlayerName;
                        playerListItem.IsReady = ev.Value.IsReady;
                        if (avatarRegistry.TryGetAvatar(ev.Value.AvatarGuid.ToGuid(), out var avatar))
                            playerListItem.PlayerAvatar = avatar.AvatarImage;
                        else
                            Log.Error("Avatar not found for {0}", ev.Value.AvatarGuid.ToString());
                    }

                    break;
                }
            }
        }

        private async UniTask AddPlayerToListView(NetworkAvatarSelection.SessionPlayerState player)
        {
            // サーバークライアントはリストに追加しない
            if (player.ClientId == NetworkManager.ServerClientId) return;

            // アバターが設定されるまで1フレーム待機
            await UniTask.Yield();

            var itemGo = await playerListItemViewPrefab.InstantiateAsync();
            itemGo.TryGetComponent(out PlayerListItemView item);

            // 各種情報を設定
            item.PlayerName = player.PlayerName;

            var avatarGuid = player.AvatarGuid.ToGuid();
            if (avatarRegistry.TryGetAvatar(avatarGuid, out var avatar))
                item.PlayerAvatar = avatar.AvatarImage;
            else
                Log.Error("Avatar not found for {0}", avatarGuid.ToString());

            scrollView.AddItem(item.transform, true);
            _playerListItems.Add(player.ClientId, item);
        }

        private void RemovePlayerFromListView(ulong clientId)
        {
            if (!_playerListItems.Remove(clientId, out var item)) return;

            scrollView.RemoveItem(item.transform);
        }
    }
}