using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Karuta.Objects;

namespace Karuta
{
    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager Instance { get; private set; }

        [SerializeField] private Theme baseTheme;
        [SerializeField] private string baseThemeName;


        private readonly List<LightJsonTheme> lightThemes = new();
        private readonly List<string> lightThemesNames = new();
        private int currentLightTheme;
        private JsonTheme currentTheme;





        public UnityEvent InitializeThemeEvent { get; } = new UnityEvent();
        public UnityEvent UpdateThemeEvent { get; } = new UnityEvent();

        private bool initialized = false;

        private void Awake()
        {
            // Be sure that there is only one instance of DecksManager
            if (Instance == null)
            {
                Instance = this;

                Initialize();

                initialized = true;
            }
            else
            {
                Destroy(gameObject); // Destroy if another DecksManager exist
            }
            DontDestroyOnLoad(gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {
            InitializeThemeEvent.Invoke();
        }

        private void Initialize()
        {
            InitLightThemes();

            currentLightTheme = PlayerPrefs.GetInt("theme", 0);

            LoadTheme();

            UpdateThemeEvent.Invoke();
        }

        public bool IsInitialized()
        {
            return initialized;
        }

        private void InitLightThemes()
        {
            lightThemes.Clear();
            lightThemesNames.Clear();
            lightThemes.Add(JsonUtility.FromJson<LightJsonTheme>(Resources.Load<TextAsset>("BaseTheme").text));
            lightThemesNames.Add(baseThemeName);

            string[] themeFiles = Directory.GetFiles(LoadManager.ThemesDirectoryPath);

            foreach (string themeFile in themeFiles)
            {
                lightThemes.Add(JsonUtility.FromJson<LightJsonTheme>(File.ReadAllText(Path.Combine(LoadManager.ThemesDirectoryPath, themeFile))));
                lightThemesNames.Add(themeFile);
            }

            foreach (LightJsonTheme theme in lightThemes)
            {
                Debug.Log(theme.mainMenuBackground);
            }
        }

        public void LoadTheme()
        {
            //currentTheme = JsonUtility.FromJson<JsonTheme>(File.ReadAllText(Path.Combine(LoadManager.ThemesDirectoryPath, lightThemesNames[currentLightTheme])));
        }

        #region Getter
        public LightJsonTheme GetCurrentLightTheme()
        {
            return lightThemes[currentLightTheme];
        }

        public int GetCurrentLightThemeInt()
        {
            return currentLightTheme;
        }

        public JsonTheme GetCurrentTheme()
        {
            return currentTheme;
        }
        #endregion Getter
    }
}