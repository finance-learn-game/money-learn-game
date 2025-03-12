using System;
using System.Collections.Generic;
using System.Linq;
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
            _newsDataList.AddRange(dirtyNewsText
                .Replace("\r\n", "\n").Split("\n")
                .Select(l =>
                {
                    Debug.Log(l);
                    return l;
                })
                .Where(l => l.StartsWith("•"))
                .Select(l =>
                {
                    var split = l.Split(':');
                    if (DateTime.TryParse(split[0], out var date))
                        return new NewsData
                        {
                            Date = date,
                            Detail = split[1].Trim()
                        };
                    return null;
                })
                .Where(d => d != null)
            );
            Debug.Log(_newsDataList.Count);
        }
    }
}