using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using SmbcApp.LearnGame.UIWidgets.Modal;
using SmbcApp.LearnGame.UIWidgets.ScrollView;
using SmbcApp.LearnGame.Utils;
using TMPro;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.Gameplay.UI.MainMenu
{
    public class ProfileListModalView : UIModal
    {
        [SerializeField] [Required] private UIScrollView profilesList;
        [SerializeField] [Required] private UIButton createProfileButton;
        [SerializeField] [Required] private UIButton startButton;
        [SerializeField] [Required] private TMP_Text currentProfileText;
        [SerializeField] [Required] private ProfileListItemView.Ref profileListItemPrefab;
        [SerializeField] [Required] private Ref profileCreateModal;
        [SerializeField] [Required] private Ref sessionJoinModal;

        [Inject] internal ProfileManager ProfileManager;

        protected override void Start()
        {
            base.Start();

            createProfileButton.OnClick
                .Subscribe(_ => ModalContainer.Push(profileCreateModal.AssetGUID, true))
                .AddTo(gameObject);
            startButton.OnClick
                .Where(_ => !string.IsNullOrEmpty(ProfileManager.CurrentProfile.CurrentValue))
                .Subscribe(_ => ModalContainer.Push(sessionJoinModal.AssetGUID, true))
                .AddTo(gameObject);

            BuildProfileList(default).AsUniTask();
            ProfileManager.AvailableProfiles
                .ObserveChanged()
                .SubscribeAwait(BuildProfileList)
                .AddTo(gameObject);

            ProfileManager.CurrentProfile
                .Subscribe(profile =>
                {
                    currentProfileText.text =
                        string.IsNullOrEmpty(profile) ? "プロファイルが選択されていません" : $"プロファイル: {profile}";
                    startButton.IsInteractable = !string.IsNullOrEmpty(profile);
                })
                .AddTo(gameObject);
        }

        private async ValueTask BuildProfileList(CollectionChangedEvent<string> ev,
            CancellationToken cancellation = new())
        {
            profilesList.Clear();
            foreach (var profile in ProfileManager.AvailableProfiles)
            {
                var item = await profileListItemPrefab.InstantiateAsync();
                item.ProfileName = profile;
                item.OnSelect
                    .Subscribe(_ => ProfileManager.SetCurrentProfile(profile))
                    .AddTo(item);
                item.OnDelete
                    .Subscribe(_ => ProfileManager.DeleteProfile(profile))
                    .AddTo(item);
                ProfileManager.CurrentProfile
                    .Subscribe(currentProfile => item.IsSelected = currentProfile == profile)
                    .AddTo(item);
                profilesList.AddItem(item.transform, true);
            }
        }
    }
}