using Sirenix.OdinInspector;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Result
{
    internal sealed class ResultPageView : Page
    {
        [SerializeField] [Required] private ResultListView resultList;

        [Inject]
        internal void Construct(IObjectResolver resolver)
        {
            resolver.Inject(resultList);
        }
    }
}