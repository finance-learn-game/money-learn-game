using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using SmbcApp.LearnGame.Infrastructure;
using SmbcApp.LearnGame.UIWidgets.ScrollView;
using Unity.Logging;
using Unity.Netcode;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class PlayerListView : MonoBehaviour
    {
        [SerializeField] [Required] private UIScrollView scrollView;
        [SerializeField] [Required] private PlayerListItemView.Ref playerListItemViewPrefab;
        [SerializeField] [Required] private AvatarRegistry avatarRegistry;
        [SerializeField] [Required] private PersistantPlayerRuntimeCollection playerRuntimeCollection;

        private readonly Dictionary<ulong, PlayerListItemView> _playerListItems = new();

        private void Start()
        {
            playerRuntimeCollection.Players.ObserveAdd()
                .SubscribeAwait((ev, _) => AddPlayerToListView(ev.Value.Value))
                .AddTo(gameObject);
            playerRuntimeCollection.Players.ObserveRemove()
                .Subscribe(ev => RemovePlayerFromListView(ev.Value.Value))
                .AddTo(gameObject);

            // 既に存在するプレイヤーをリストに追加
            foreach (var (_, player) in playerRuntimeCollection.Players)
                AddPlayerToListView(player).Forget();
        }

        private async UniTask AddPlayerToListView(PersistantPlayer player)
        {
            // サーバークライアントはリストに追加しない
            if (player.OwnerClientId == NetworkManager.ServerClientId) return;

            // アバターが設定されるまで1フレーム待機
            await UniTask.Yield();

            var itemGo = await playerListItemViewPrefab.InstantiateAsync();
            itemGo.TryGetComponent(out PlayerListItemView item);

            item.PlayerName = player.Name.Value;

            var avatarGuid = player.AvatarGuidState.AvatarGuid.Value.ToGuid();
            if (avatarRegistry.TryGetAvatar(avatarGuid, out var avatar))
                item.PlayerAvatar = avatar.AvatarImage;
            else
                Log.Error("Avatar not found for {0}", avatarGuid.ToString());

            scrollView.AddItem(item.transform, true);
            _playerListItems.Add(player.OwnerClientId, item);
        }

        private void RemovePlayerFromListView(PersistantPlayer player)
        {
            if (!_playerListItems.Remove(player.OwnerClientId, out var item)) return;

            scrollView.RemoveItem(item.transform);
        }
    }
}