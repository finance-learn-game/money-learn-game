using UnityEngine;

namespace SmbcApp.LearnGame.UIWidgets.Spinner
{
    /// <summary>
    ///     読み込み中を表すスピナーUI
    /// </summary>
    public sealed class UISpinner : MonoBehaviour
    {
        public bool IsVisible
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }
    }
}