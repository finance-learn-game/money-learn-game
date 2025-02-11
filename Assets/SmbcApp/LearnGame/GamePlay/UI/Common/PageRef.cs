using System;
using UnityScreenNavigator.Runtime.Core.Page;

namespace SmbcApp.LearnGame.Gameplay.UI.Common
{
    [Serializable]
    public class PageRef : ComponentReference<Page>
    {
        public PageRef(string guid) : base(guid)
        {
        }
    }
}