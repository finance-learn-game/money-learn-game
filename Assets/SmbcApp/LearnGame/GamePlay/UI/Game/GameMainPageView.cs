using Sirenix.OdinInspector;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class GameMainPageView : Page
    {
        [SerializeField] [Required] private GameTimeTextView timeTextView;

        [Inject]
        internal void Construct(IObjectResolver resolver)
        {
            resolver.Inject(timeTextView);
        }
    }
}