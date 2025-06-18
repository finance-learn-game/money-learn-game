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
        [SerializeField] [Required] private TMP_Text minDateText;
        [SerializeField] [Required] private TMP_Text maxDateText;

        [Inject] internal NetworkGameTurn GameTurn;

        private void Start()
        {
            var duration = GameTurn.GameRange.End.AddMonths(1) - GameTurn.GameRange.Start;
            minDateText.text = GameTurn.GameRange.Start.ToString("yyyy/MM");
            maxDateText.text = GameTurn.GameRange.End.ToString("yyyy/MM");
            
            GameTurn.OnChangeTime
                .Prepend(GameTurn.CurrentTime)
                .Subscribe(time =>
                {
                    var diff = time - GameTurn.GameRange.Start;
                    Debug.Log($"{time}, {GameTurn.GameRange.Start}, {diff}, {duration}");
                    dateSlider.value = (float)diff.TotalDays / (float)duration.TotalDays;
                    timeText.text = time.ToString("yyyy/MM");
                })
                .AddTo(gameObject);
        }
    }
}