using System;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using TMPro;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Result
{
    internal sealed class ResultTownViewPageView : Page
    {
        [SerializeField] [Required] private UIButton returnButton;
        [SerializeField] [Required] private TMP_Text userNameText;

        [Inject] internal PageContainer PageContainer;

        private void Start()
        {
            returnButton.OnClick
                .Subscribe(_ => PageContainer.Pop(true))
                .AddTo(this);
        }

        public void Configure(string playerName)
        {
            userNameText.text = $"{playerName}さんの街";
        }

        [Serializable]
        public class Ref : ComponentReference<ResultTownViewPageView>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}