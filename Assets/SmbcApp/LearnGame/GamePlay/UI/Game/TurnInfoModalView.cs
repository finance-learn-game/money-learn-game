using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.UIWidgets.Modal;
using TMPro;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class TurnInfoModalView : UIModal
    {
        [SerializeField] [Required] private TMP_Text dateText;
        [SerializeField] [Required] private TMP_Text salaryText;

        [Inject] internal NetworkGameTurn GameTurn;

        protected override void Start()
        {
            base.Start();

            var date = GameTurn.CurrentTime;
            dateText.text = $"{date:yyyy/MM/dd} になりました";
        }
    }
}