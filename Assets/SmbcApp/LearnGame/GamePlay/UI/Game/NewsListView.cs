using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Data;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.UIWidgets.ScrollView;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class NewsListView : MonoBehaviour
    {
        [SerializeField] [Required] private UIScrollView newsListView;
        [SerializeField] [Required] private NewsListItemView.Ref newsListItemPrefab;

        [Inject] internal NetworkGameTurn GameTurn;
        [Inject] internal MasterData MasterData;

        private void Start()
        {
            var newsTable = MasterData.DB.NewsDataTable;

            var date = GameTurn.CurrentTime;
            var newsList = newsTable.FindByYearAndMonth((date.Year, date.Month));

            newsListView.Clear();
            UniTask.WhenAll(newsList.Select(async news =>
            {
                var item = await newsListItemPrefab.InstantiateAsync().BindTo(gameObject);
                item.DateText = $"{news.Year} 年 {news.DirtyDate}";
                item.ContentText = news.Detail;
                newsListView.AddItem(item.transform, true);
            })).Forget();
        }
    }
}