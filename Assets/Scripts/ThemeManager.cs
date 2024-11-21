using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Karuta.ScriptableObjects;
using static Karuta.ScriptableObjects.JsonObjects;

namespace Karuta
{
    public class ThemeManager : MonoBehaviour
    {
        [SerializeField] private Theme baseTheme;

        private readonly List<LightJsonTheme> themes = new();
        private JsonTheme currentTheme;

        public UnityEvent UpdateTheme { get; } = new UnityEvent();

        private void OnEnable()
        {
            


        }

        private void Initialize()
        {
            InitLightThemes();

            LoadTheme(PlayerPrefs.GetInt("theme", 0));

            ThemeApply();
        }

        private void InitLightThemes()
        {
            themes.Clear();
            themes.Add(JsonUtility.FromJson<LightJsonTheme>(Resources.Load<TextAsset>("BaseTheme").text));

            string[] themeFiles = Directory.GetFiles(LoadManager.ThemesDirectoryPath);

            foreach (string themeFile in themeFiles) 
            {
                themes.Add(JsonUtility.FromJson<LightJsonTheme>(File.ReadAllText(Path.Combine(LoadManager.ThemesDirectoryPath, themeFile))));
            }
        }

        private void LoadTheme(int index)
        {
            if (index > 0 && index < themes.Count - 1)
            {
                string[] themeFiles = Directory.GetFiles(LoadManager.ThemesDirectoryPath);

                currentTheme = JsonUtility.FromJson<JsonTheme>(File.ReadAllText(Path.Combine(LoadManager.ThemesDirectoryPath, themeFiles[index - 1])));
            }
            else
            {
                currentTheme = JsonUtility.FromJson<JsonTheme>(Resources.Load<TextAsset>("BaseTheme").text);
            }
        }

        public void NextTheme(bool right)
        {

        }

        private void SoftThemeApply()
        {

        }

        private void ThemeApply()
        {
            UpdateTheme.Invoke();
        }

        public JsonTheme GetTheme()
        {
            return currentTheme;
        }
    }
}