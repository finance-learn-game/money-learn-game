using System;
using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace SmbcApp.LearnGame.GamePlay.UI.Result
{
    internal sealed class ResultListItemView : MonoBehaviour
    {
        [SerializeField] [Required] private Image thumbnailImage;
        [SerializeField] [Required] private TMP_Text playerNameText;
        [SerializeField] [Required] private TMP_Text scoreText;
        [SerializeField] [Required] private UIButton viewTownButton;

        public Observable<Unit> OnViewTownButtonClick => viewTownButton.OnClick;

        public async UniTask Configure(
            AssetReferenceSprite thumbnailRef,
            string playerName, string avatarName, int point, int balance
        )
        {
            playerNameText.text = $"{playerName} - {avatarName}";
            scoreText.text = $"街ポイント: {point} / 所持金: {balance}";
            thumbnailImage.sprite = thumbnailRef.IsValid()
                ? await thumbnailRef.OperationHandle.Task as Sprite
                : await thumbnailRef.LoadAssetAsync().BindTo(gameObject);
        }

        [Serializable]
        public class Ref : ComponentReference<ResultListItemView>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}