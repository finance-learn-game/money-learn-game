using Sirenix.OdinInspector;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class ServerInGameMainPageView : Page
    {
        [SerializeField] [Required] private GameTimeTextView gameTime;

        [Inject]
        internal void Construct(IObjectResolver resolver)
        {
            resolver.Inject(gameTime);
        }
    }
}