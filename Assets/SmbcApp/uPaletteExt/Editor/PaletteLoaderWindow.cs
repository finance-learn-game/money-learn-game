using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using uPalette.Runtime.Core;

namespace SmbcApp.uPaletteExt.Editor
{
    internal sealed class PaletteLoaderWindow : OdinEditorWindow
    {
        [SerializeField] private PaletteStore paletteStore;

        [SerializeField] [Sirenix.OdinInspector.FilePath(Extensions = "json")] [ShowIf(nameof(paletteStore))]
        private string themeJsonFile;

        [ShowIf(nameof(paletteStore))]
        [Button]
        private void Load()
        {
            UniTask.Void(async () =>
            {
                var data = await LoadFromJson();
                ApplyPalette(data);
                Debug.Log("Loaded");
            });
        }

        private void ApplyPalette(MaterialThemeJson data)
        {
            var palette = paletteStore.ColorPalette;

            // テーマを追加
            var themeName2Theme = palette.Themes.ToDictionary(v => v.Value.Name.Value, v => v.Value);
            foreach (var (schemeName, _) in data.schemes)
            {
                if (themeName2Theme.ContainsKey(schemeName)) continue;
                var theme = palette.AddTheme();
                theme.Name.Value = schemeName;
                themeName2Theme.Add(schemeName, theme);
            }

            foreach (var (_, theme) in themeName2Theme.Where(v => !data.schemes.ContainsKey(v.Key)))
                palette.RemoveTheme(theme.Id);

            var colorNames = data.schemes.First().Value.Keys.Select(EntryName);
            var entryName2Entry = palette.Entries.ToDictionary(
                v => v.Value.Name.Value,
                v => v.Value
            );
            foreach (var colorName in colorNames)
            {
                if (!entryName2Entry.TryGetValue(colorName, out var entry))
                {
                    entry = palette.AddEntry();
                    entry.Name.Value = colorName;
                }

                foreach (var (schemeName, colors) in data.schemes)
                {
                    var theme = themeName2Theme[schemeName];
                    var color = colors[colorName.Split("/").Last()];
                    if (ColorUtility.TryParseHtmlString(color, out var c))
                        entry.Values[theme.Id].Value = c;
                }
            }

            EditorUtility.SetDirty(paletteStore);
        }

        private static string EntryName(string entryName)
        {
            return $"MaterialTheme/{entryName}";
        }

        private async UniTask<MaterialThemeJson> LoadFromJson()
        {
            await using var jsonStream = new FileStream(themeJsonFile, FileMode.Open);
            using var reader = new StreamReader(jsonStream);
            var data = JsonConvert.DeserializeObject<MaterialThemeJson>(await reader.ReadToEndAsync());
            return data;
        }

        [MenuItem("Tools/uPaletteExt/Palette Loader")]
        private static void OpenWindow()
        {
            GetWindow<PaletteLoaderWindow>().Show();
        }

        internal record MaterialThemeJson
        {
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once CollectionNeverUpdated.Global
            public Dictionary<string, Dictionary<string, string>> schemes;
        }
    }
}