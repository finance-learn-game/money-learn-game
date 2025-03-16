using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using MasterMemory;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace SmbcApp.LearnGame.Data.Editor
{
    /// <summary>
    ///     ニュースのデータを読み込むことのできるエディタ拡張
    /// </summary>
    internal sealed class NewsDataLoaderEditorWindow : OdinEditorWindow
    {
        [SerializeField] [TextArea] private string dirtyNewsText;

        [ReadOnly] [ShowInInspector] private readonly List<NewsData> _newsDataList = new();

        [MenuItem("Tools/Master Data/News Data Loader")]
        private static void OpenWindow()
        {
            GetWindow<NewsDataLoaderEditorWindow>().Show();
        }

        [Button]
        private void Parse()
        {
            _newsDataList.Clear();

            var lines = dirtyNewsText.Replace("\r\n", "\n").Split("\n");

            var year = -1;
            var currNews = new StringBuilder();
            foreach (var line in from line in lines select line.Trim())
                if (line[0] == '【')
                {
                    year = int.Parse(line[1..5]);
                    AddNews();
                }
                else if (line.Last() == '月')
                {
                    AddNews();
                }
                else if (year != -1)
                {
                    if (line[0] == '•') AddNews();
                    currNews.Append(line);
                }

            AddNews();

            return;

            void AddNews()
            {
                if (year == -1 || currNews.Length <= 1) return;

                var split = currNews.ToString().Split(":");
                if (split.Length != 2) return;

                var dirtyDate = split[0][2..].Trim();
                var month = int.Parse(dirtyDate.Split("月")[0]);
                _newsDataList.Add(new NewsData
                {
                    Id = _newsDataList.Count,
                    Year = year,
                    Month = month,
                    DirtyDate = dirtyDate,
                    Detail = split[1].Trim()
                });
                currNews.Clear();
            }
        }

        [Button]
        private async UniTask SaveBinary()
        {
            Debug.Log("Generating Binary");
            var path = Path.Combine(MasterData.BinaryDirectory, MasterData.BinaryFileName);
            MemoryDatabase db;
            await using (var fs = new FileStream(path, FileMode.Open))
            {
                var buf = new byte[fs.Length];
                _ = await fs.ReadAsync(buf);
                db = new MemoryDatabase(buf);
            }

            var builder = new DatabaseBuilder();

            builder.Append(_newsDataList);
            builder.Append(db.OrganizationDataTable.All);
            builder.Append(db.StockDataTable.All);

            await using var stream = new FileStream(path,
                FileMode.Create);
            builder.WriteToStream(stream);
            Debug.Log("Binary generated and saved");
        }
    }
}