using UnityEngine;
using UnityEngine.Events;
using Karuta.Objects;
using Karuta.Commons;

namespace Karuta
{
    [System.Serializable]
    public class Background
    {
        public bool isTexture;
        public Texture texture;
        public string videoPath;
    }

    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager Instance { get; private set; }

        [SerializeField] private ThemeApplier applier;

        [SerializeField] private BaseTheme baseTheme;
        [SerializeField] private string baseThemeName;



        /*
        private readonly List<LightJsonTheme> lightThemes = new();
        private readonly List<string> lightThemesNames = new();*/
        private int currentThemeIndex;
        private Theme currentTheme = new();

        public UnityEvent InitializeThemeEvent { get; } = new();
        public UnityEvent UpdateThemeEvent { get; } = new();

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
            baseTheme.Init();

            InitLightThemes();

            currentThemeIndex = PlayerPrefs.GetInt("theme", 0);

            LoadTheme();

            UpdateThemeEvent.Invoke();
        }

        public bool IsInitialized()
        {
            return initialized;
        }

        private void InitLightThemes()
        {
            /*
            lightThemes.Clear();
            lightThemesNames.Clear();

            lightThemes.Add(new()
            {
                mainBackground = baseTheme.GetMainMenuBackgroundPath(),
                decksSelectionBackground = baseTheme.GetDecksChoiceBackgroundPath()
            });
            lightThemesNames.Add(baseThemeName);

            string[] themeFiles = Directory.GetFiles(LoadManager.ThemesDirectoryPath);

            foreach (string themeFile in themeFiles)
            {
                lightThemes.Add(JsonUtility.FromJson<LightJsonTheme>(File.ReadAllText(Path.Combine(LoadManager.ThemesDirectoryPath, themeFile))));
                lightThemesNames.Add(themeFile);
            }

            foreach (LightJsonTheme theme in lightThemes)
            {
                Debug.Log(theme.mainBackground);
            }*/
        }

        public void LoadTheme()
        {
            if (currentThemeIndex > 0)
            {
                //currentTheme = new(JsonUtility.FromJson<JsonTheme>(File.ReadAllText(Path.Combine(LoadManager.ThemesDirectoryPath, lightThemesNames[currentThemeIndex]))));
            }
        }

        #region Theme Getter
        /*
        public LightJsonTheme GetCurrentLightTheme()
        {
            return lightThemes[currentThemeIndex];
        }*/

        public int GetCurrentLightThemeInt()
        {
            return currentThemeIndex;
        }



        public Theme GetCurrentTheme()
        {
            return currentTheme;
        }

        public BaseTheme GetBaseTheme()
        {
            return baseTheme;
        }
        #endregion Theme Getter





#if UNITY_EDITOR
        private void OnValidate()
        {
            if (applier != null)
            {
                applier.VisualizeApplication(currentTheme, baseTheme);
            }
        }
#endif
    }
}