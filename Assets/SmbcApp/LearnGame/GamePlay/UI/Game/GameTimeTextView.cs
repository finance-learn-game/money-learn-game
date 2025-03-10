using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using TMPro;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class GameTimeTextView : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text timeText;

        [Inject] internal NetworkGameTurn GameTurn;

        private void Start()
        {
            GameTurn.OnChangeAsObservable()
                .Subscribe(time => timeText.text = time.ToString("yyyy/MM/dd"))
                .AddTo(gameObject);
        }
    }
}