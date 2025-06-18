using SmbcApp.LearnGame.GamePlay.Configuration;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SmbcApp.LearnGame.GamePlay.UI.MainMenu
{
    internal sealed class ConnectionMethodView : MonoBehaviour, IPointerClickHandler
    {
        private static bool _isLeft;
        [SerializeField] private RectTransform knob;

        private bool IsLeft
        {
            get => _isLeft;
            set
            {
                _isLeft = value;

                knob.anchorMin = new Vector2(value ? 0 : 1, knob.anchorMin.y);
                knob.anchorMax = new Vector2(value ? 0 : 1, knob.anchorMax.y);
                knob.pivot = new Vector2(value ? 0 : 1, knob.pivot.y);
                knob.anchoredPosition = new Vector2(value ? 2.5f : -2.5f, 0);

                GameConfiguration.Instance.SetConnectionMethod(value
                    ? GameConfiguration.ConnectionMethodType.Direct
                    : GameConfiguration.ConnectionMethodType.Relay
                );
            }
        }

        private void Start()
        {
#if UNITY_WEBGL
            IsLeft = false;
#endif
            IsLeft = _isLeft;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
#if !UNITY_WEBGL
            IsLeft = !IsLeft;
#endif
        }
    }
}