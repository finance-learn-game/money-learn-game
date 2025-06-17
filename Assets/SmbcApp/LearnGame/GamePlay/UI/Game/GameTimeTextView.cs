using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class GameTimeTextView : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text timeText;
        [SerializeField] [Required] private Slider dateSlider;

        [Inject] internal NetworkGameTurn GameTurn;

        private void Start()
        {
            var duration = GameTurn.GameRange.End - GameTurn.GameRange.Start;
            GameTurn.OnChangeTime
                .Prepend(GameTurn.CurrentTime)
                .Subscribe(time =>
                {
                    var diff = time - GameTurn.GameRange.Start;
                    dateSlider.value = (float)diff.TotalSeconds / (float)duration.TotalSeconds;
                    timeText.text = time.ToString("yyyy/MM");
                })
                .AddTo(gameObject);
        }
    }
}