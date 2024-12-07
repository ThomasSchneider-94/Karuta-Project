using UnityEngine;
using UnityEngine.UI;
using Karuta.Objects;


namespace Karuta.Commons
{
    public class ThemeApplier : MonoBehaviour
    {
        ThemeManager themeManager;

        [SerializeField] private bool inGame;

        [Header("Backgrounds")]
        [SerializeField] private Image mainMenuBackground;
        [SerializeField] private Image decksChoiceBackground;
        [SerializeField] private Image gameBackground;


        private int previousTheme;




        private void OnEnable()
        {
            themeManager = ThemeManager.Instance;

            themeManager.UpdateThemeEvent.AddListener(SoftApplyTheme);

            // Initialize
            if (themeManager.IsInitialized())
            {
                ApplyTheme();
            }
            else
            {
                themeManager.InitializeThemeEvent.AddListener(ApplyTheme);
            }
        }

        public void ApplyThemeOnClose()
        {
            /*
            if (themeManager.GetCurrentThemeInt() != previousTheme)
            {
                previousTheme = themeManager.GetCurrentThemeInt();
                ApplyTheme();
            }
            else
            {
                SoftApplyTheme();
            }*/
        }

        public void ApplyTheme()
        {
            JsonTheme currentTheme = themeManager.GetCurrentTheme();

            ApplyBackgrounds(currentTheme);

            Debug.Log("Apply");
        }

        private void ApplyBackgrounds(JsonTheme theme)
        {
            if (inGame)
            {

            }
            else
            {

            }
        }








        private void SoftApplyTheme()
        {

        }
    }
}