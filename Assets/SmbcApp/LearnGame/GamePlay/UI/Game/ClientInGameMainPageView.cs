﻿using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.UIWidgets.Button;
using SmbcApp.LearnGame.UIWidgets.Modal;
using Unity.Netcode;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class ClientInGameMainPageView : Page
    {
        [SerializeField] [Required] private GameTimeTextView timeTextView;
        [SerializeField] [Required] private BalanceTextView balanceTextView;
        [SerializeField] [Required] private PointTextView pointTextView;
        [SerializeField] [Required] private UIButton openStockModalButton;
        [SerializeField] [Required] private UIButton turnEndButton;
        [SerializeField] [Required] private UIButton openTurnInfoModalButton;
        [SerializeField] [Required] private UIButton openTownPartsModalButton;
        [SerializeField] [Required] private UIModal.Ref stockModalPrefab;
        [SerializeField] [Required] private UIModal.Ref turnInfoModalPrefab;
        [SerializeField] [Required] private UIModal.Ref townPartsModalPrefab;

        private NetworkGameTurn _gameTurn;
        private ModalContainer _modalContainer;

        private void Start()
        {
            var clientId = NetworkManager.Singleton.LocalClientId;

            openStockModalButton.OnClick
                .Subscribe(_ => _modalContainer.Push(stockModalPrefab.AssetGUID, true))
                .AddTo(gameObject);
            turnEndButton.OnClick
                .Subscribe(_ => _gameTurn.ToggleTurnEndRpc(clientId))
                .AddTo(gameObject);
            openTownPartsModalButton.OnClick
                .Subscribe(_ => _modalContainer.Push(townPartsModalPrefab.AssetGUID, true))
                .AddTo(gameObject);
            _gameTurn.ObserveChangeTurnEnd(clientId)
                .Prepend(false)
                .Subscribe(turnEnd =>
                {
                    turnEndButton.Text = turnEnd ? "待機中" : "次のターンへ";
                    openStockModalButton.IsInteractable = !turnEnd;
                    openTurnInfoModalButton.IsInteractable = !turnEnd;
                    openTownPartsModalButton.IsInteractable = !turnEnd;
                })
                .AddTo(gameObject);
            _gameTurn.OnChangeTime
                .Prepend(_gameTurn.CurrentTime)
                .Subscribe(_ => _modalContainer.Push(turnInfoModalPrefab.AssetGUID, true))
                .AddTo(gameObject);
            openTurnInfoModalButton.OnClick
                .Subscribe(_ => _modalContainer.Push(turnInfoModalPrefab.AssetGUID, true))
                .AddTo(gameObject);
        }

        [Inject]
        internal void Construct(IObjectResolver resolver, ModalContainer modalContainer, NetworkGameTurn gameTurn)
        {
            _modalContainer = modalContainer;
            _gameTurn = gameTurn;

            resolver.Inject(timeTextView);
            resolver.Inject(balanceTextView);
            resolver.Inject(pointTextView);
        }
    }
}