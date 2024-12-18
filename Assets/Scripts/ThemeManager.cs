using UnityEngine;
using UnityEngine.Events;
using Karuta.Objects;
using Karuta.Commons;

namespace Karuta
{
    [System.Serializable]
    public class Background
    {
        public bool ignore;
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
        #endregion Theme Getter

        #region Theme Component Getter

        #region Backgrounds
        public Background GetMainBackground()
        {
            return GetBackgroundFromString(currentTheme.GetMainBackground(), baseTheme.GetMainBackground());
        }

        public Background GetDecksSelectionBackground()
        {
            return GetBackgroundFromString(currentTheme.GetDecksSelectionBackground(), baseTheme.GetDecksSelectionBackground());
        }

        public Background GetGameBackground()
        {
            return GetBackgroundFromString(currentTheme.GetGameBackground(), baseTheme.GetGameBackground());
        }

        private static Background GetBackgroundFromString(string backgroundString, Background defaultBackground)
        {
            if (string.IsNullOrEmpty(backgroundString))
            {
                return defaultBackground;
            }
            bool isTexture = backgroundString.Split(".")[^1] == "png";

            return new()
            {
                isTexture = isTexture,
                texture = isTexture ? LoadManager.LoadThemeVisual(backgroundString) : null,
                videoPath = isTexture ? null : backgroundString,
            };

        }
        #endregion Backgrounds

        #region Question Color
        public Color GetQuestionPanelColor()
        {
            return GetColorFromString(currentTheme.questionPanelColor, baseTheme.questionPanelColor);
        }

        public Color GetQuestionTextColor()
        {
            return GetColorFromString(currentTheme.questionTextColor, baseTheme.questionTextColor);
        }

        public Color GetQuestionNumberPanelColor()
        {
            return GetColorFromString(currentTheme.questionNumberPanelColor, baseTheme.questionNumberPanelColor);
        }

        public Color GetQuestionNumberSelectedPanelColor()
        {
            return GetColorFromString(currentTheme.questionNumberSelectedPanelColor, baseTheme.questionNumberSelectedPanelColor);
        }

        public Color GetQuestionNumberTextColor()
        {
            return GetColorFromString(currentTheme.questionNumberTextColor, baseTheme.questionNumberTextColor);
        }
        #endregion Question Color

        #region Download Only Toggle
        public Color GetDownloadOnlyToggleLabelColor()
        {
            return GetColorFromString(currentTheme.downloadOnlyToggleLabelColor, baseTheme.downloadOnlyToggleLabelColor);
        }

        public Color GetDownloadOnlyToggleLabelOutlineColor()
        {
            return GetColorFromString(currentTheme.downloadOnlyToggleLabelOutlineColor, baseTheme.downloadOnlyToggleLabelOutlineColor);
        }

        public Color GetDownloadOnlyToggleCheckBoxOutlineColor()
        {
            return GetColorFromString(currentTheme.downloadOnlyToggleCheckBoxOutlineColor, baseTheme.downloadOnlyToggleCheckBoxOutlineColor);
        }

        public Color GetDownloadOnlyToggleCheckBoxColor()
        {
            return GetColorFromString(currentTheme.downloadOnlyToggleCheckBoxColor, baseTheme.downloadOnlyToggleCheckBoxColor);
        }

        public Color GetDownloadOnlyToggleCheckMarkColor()
        {
            return GetColorFromString(currentTheme.downloadOnlyToggleCheckMarkColor, baseTheme.downloadOnlyToggleCheckMarkColor);
        }
        #endregion Download Only Toggle

        #region Category Buttons Color
        public Color GetCategoryButtonOutlineColor()
        {
            return GetColorFromString(currentTheme.categoryButtonOutlineColor, baseTheme.categoryButtonOutlineColor);
        }

        public Color GetCategoryButtonInsdeColor()
        {
            return GetColorFromString(currentTheme.categoryButtonInsideColor, baseTheme.categoryButtonInsideColor);
        }
        #endregion Category Buttons Color

        #region Arrow Buttons Color
        public Color GetArrowButtonOutsideColor()
        {
            return GetColorFromString(currentTheme.arrowButtonOutsideColor, baseTheme.arrowButtonOutsideColor);
        }

        public Color GetArrowButtonInsideColor()
        {
            return GetColorFromString(currentTheme.arrowButtonInsideColor, baseTheme.arrowButtonInsideColor);
        }

        public Color GetArrowButtonTextColor()
        {
            return GetColorFromString(currentTheme.arrowButtonTextColor, baseTheme.arrowButtonTextColor);
        }

        public Color GetReverseArrowButtonOutsideColor()
        {
            return GetColorFromString(currentTheme.reverseArrowButtonOutsideColor, baseTheme.reverseArrowButtonOutsideColor);
        }

        public Color GetReverseArrowButtonInsideColor()
        {
            return GetColorFromString(currentTheme.reverseArrowButtonInsideColor, baseTheme.reverseArrowButtonInsideColor);
        }

        public Color GetReverseArrowButtonTextColor()
        {
            return GetColorFromString(currentTheme.reverseArrowButtonTextColor, baseTheme.reverseArrowButtonTextColor);
        }
        #endregion Arrow Buttons Color

        #region Option Buttons Color
        public Color GetOptionButtonOutlineColor()
        {
            return GetColorFromString(currentTheme.optionButtonOutlineColor, baseTheme.optionButtonOutlineColor);
        }

        public Color GetOptionButtonInsideColor()
        {
            return GetColorFromString(currentTheme.optionButtonInsideColor, baseTheme.optionButtonInsideColor);
        }

        public Color GetOptionButtonIconColor()
        {
            return GetColorFromString(currentTheme.optionButtonIconColor, baseTheme.optionButtonIconColor);
        }
        #endregion Option Buttons Color

        #region Option Toggles Color
        public Color GetOptionsTogglesLabelColor()
        {
            return GetColorFromString(currentTheme.optionsTogglesLabelColor, baseTheme.optionsTogglesLabelColor);
        }

        public Color GetOptionsTogglesLabelOutlineColor()
        {
            return GetColorFromString(currentTheme.optionsTogglesLabelOutlineColor, baseTheme.optionsTogglesLabelOutlineColor);
        }

        public Color GetOptionsTogglesCheckBoxOutlineColor()
        {
            return GetColorFromString(currentTheme.optionsTogglesCheckBoxOutlineColor, baseTheme.optionsTogglesCheckBoxOutlineColor);
        }

        public Color GetOptionsTogglesCheckBoxColor()
        {
            return GetColorFromString(currentTheme.optionsTogglesCheckBoxColor, baseTheme.optionsTogglesCheckBoxColor);
        }

        public Color GetOptionsTogglesCheckMarkColor()
        {
            return GetColorFromString(currentTheme.optionsTogglesCheckMarkColor, baseTheme.optionsTogglesCheckMarkColor);
        }
        #endregion Option Toggles Color

        #region Close Buttons Color
        public Color GetCloseButtonOutlineColor()
        {
            return GetColorFromString(currentTheme.closeButtonOutlineColor, baseTheme.closeButtonOutlineColor);
        }

        public Color GetCloseButtonInsideColor()
        {
            return GetColorFromString(currentTheme.closeButtonInsideColor, baseTheme.closeButtonInsideColor);
        }

        public Color GetCloseButtonIconColor()
        {
            return GetColorFromString(currentTheme.closeButtonIconColor, baseTheme.closeButtonIconColor);
        }
        #endregion Option Buttons Color

        #region Panel Buttons Color
        public Color GetPanelButtonOutlineColor()
        {
            return GetColorFromString(currentTheme.panelButtonOutlineColor, baseTheme.panelButtonOutlineColor);
        }

        public Color GetPanelButtonInsideColor()
        {
            return GetColorFromString(currentTheme.panelButtonInsideColor, baseTheme.panelButtonInsideColor);
        }

        public Color GetPanelButtonTextColor()
        {
            return GetColorFromString(currentTheme.panelButtonTextColor, baseTheme.panelButtonTextColor);
        }
        #endregion Panel Buttons Color

        #region Option Panel Color
        public Color GetOptionPanelColor()
        {
            return GetColorFromString(currentTheme.optionPanelColor, baseTheme.optionPanelColor);
        }

        public Color GetOptionPanelBorderColor()
        {
            return GetColorFromString(currentTheme.optionPanelBorderColor, baseTheme.optionPanelBorderColor);
        }
        #endregion Option Panel Color

        #region Deck Selection Button Color
        public Color GetDeckSelectionButtonOutlineColor()
        {
            return GetColorFromString(currentTheme.deckSelectionButtonOutlineColor, baseTheme.deckSelectionButtonOutlineColor);
        }

        public Color GetDeckSelectionButtonInsideColor()
        {
            return GetColorFromString(currentTheme.deckSelectionButtonInsideColor, baseTheme.deckSelectionButtonInsideColor);
        }

        public Color GetDeckSelectionButtonTextColor()
        {
            return GetColorFromString(currentTheme.deckSelectionButtonTextColor, baseTheme.deckSelectionButtonTextColor);
        }
        #endregion Deck Selection Button Color

        #region Deck Download Buttons Color
        public Color GetDeckDownloadButtonOutlineColor()
        {
            return GetColorFromString(currentTheme.deckDownloadButtonOutlineColor, baseTheme.deckDownloadButtonOutlineColor);
        }

        public Color GetDeckDownloadButtonInsideColor()
        {
            return GetColorFromString(currentTheme.deckDownloadButtonInsideColor, baseTheme.deckDownloadButtonInsideColor);
        }
        #endregion Deck Download Buttons Color

        #region Select All Toggle Color
        public Color GetSelectAllToggleLabelColor()
        {
            return GetColorFromString(currentTheme.selectAllToggleLabelColor, baseTheme.selectAllToggleLabelColor);
        }

        public Color GetSelectAllToggleLabelOutlineColor()
        {
            return GetColorFromString(currentTheme.selectAllToggleLabelOutlineColor, baseTheme.selectAllToggleLabelOutlineColor);
        }

        public Color GetSelectAllToggleCheckBoxOutlineColor()
        {
            return GetColorFromString(currentTheme.selectAllToggleCheckBoxOutlineColor, baseTheme.selectAllToggleCheckBoxOutlineColor);
        }

        public Color GetSelectAllToggleCheckBoxColor()
        {
            return GetColorFromString(currentTheme.selectAllToggleCheckBoxColor, baseTheme.selectAllToggleCheckBoxColor);
        }

        public Color GetSelectAllToggleCheckMarkColor()
        {
            return GetColorFromString(currentTheme.selectAllToggleCheckMarkColor, baseTheme.selectAllToggleCheckMarkColor);
        }
        #endregion Select All Toggle Color

        #region Deck Download Toggles Color
        public Color GetDeckDownloadTogglesLabelColor()
        {
            return GetColorFromString(currentTheme.deckDownloadTogglesLabelColor, baseTheme.deckDownloadTogglesLabelColor);
        }

        public Color GetDeckDownloadTogglesLabelOutlineColor()
        {
            return GetColorFromString(currentTheme.deckDownloadTogglesLabelOutlineColor, baseTheme.deckDownloadTogglesLabelOutlineColor);
        }

        public Color GetDeckDownloadTogglesCheckBoxOutlineColor()
        {
            return GetColorFromString(currentTheme.deckDownloadTogglesCheckBoxOutlineColor, baseTheme.deckDownloadTogglesCheckBoxOutlineColor);
        }

        public Color GetDeckDownloadTogglesCheckBoxColor()
        {
            return GetColorFromString(currentTheme.deckDownloadTogglesCheckBoxColor, baseTheme.deckDownloadTogglesCheckBoxColor);
        }

        public Color GetDeckDownloadTogglesCheckMarkColor()
        {
            return GetColorFromString(currentTheme.deckDownloadTogglesCheckMarkColor, baseTheme.deckDownloadTogglesCheckMarkColor);
        }

        public Color GetDeckDownloadTogglesBackgroundColor()
        {
            return GetColorFromString(currentTheme.deckDownloadTogglesBackgroundColor, baseTheme.deckDownloadTogglesBackgroundColor);
        }
        #endregion Deck Download Toggles Color

        #region Indication Arrows Colors
        public Color GetFoundArrowOutlineColor()
        {
            return GetColorFromString(currentTheme.foundArrowOutlineColor, baseTheme.foundArrowOutlineColor);
        }

        public Color GetFoundArrowInsideColor()
        {
            return GetColorFromString(currentTheme.foundArrowInsideColor, baseTheme.foundArrowInsideColor);
        }

        public Color GetFoundArrowTextColor()
        {
            return GetColorFromString(currentTheme.foundArrowTextColor, baseTheme.foundArrowTextColor);
        }

        public Color GetNotFoundArrowOutlineColor()
        {
            return GetColorFromString(currentTheme.notFoundArrowOutlineColor, baseTheme.notFoundArrowOutlineColor);
        }

        public Color GetNotFoundArrowInsideColor()
        {
            return GetColorFromString(currentTheme.notFoundArrowInsideColor, baseTheme.notFoundArrowInsideColor);
        }

        public Color GetNotFoundArrowTextColor()
        {
            return GetColorFromString(currentTheme.notFoundArrowTextColor, baseTheme.notFoundArrowTextColor);
        }
        #endregion Indication Arrows Colors



        private static Color GetColorFromString(string colorString, Color defaultColor)
        {
            if (ColorUtility.TryParseHtmlString(colorString, out Color color))
            {
                return color;
            }
            return defaultColor;
        }
        #endregion Theme Component Getter




#if UNITY_EDITOR
        private void OnValidate()
        {
            if (applier != null)
            {
                applier.VisualizeApplication(this);
            }
        }
#endif
    }
}