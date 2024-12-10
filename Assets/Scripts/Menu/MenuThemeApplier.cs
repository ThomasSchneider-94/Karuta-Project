using Karuta.Commons;
using Karuta.Objects;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Karuta.Menu
{
    public class MenuThemeApplier : ThemeApplier
    {
        [Header("Backgrounds")]
        [SerializeField] private Image mainMenuBackgroundImage;
        [SerializeField] private VideoPlayer mainMenuBackgroundVideo;
        [SerializeField] private Image decksChoiceBackgroundImage;
        [SerializeField] private VideoPlayer decksChoiceBackgroundVideo;

        protected override void ApplyBackgrounds()
        {
            string mainMenuPath = themeManager.GetMainMenuBackgroundPath();

            if (mainMenuPath.Split(".")[^1] == "png")
            {
                mainMenuBackgroundImage.sprite = LoadManager.LoadThemeVisual(mainMenuPath);
            }

            string deckSelectionMenuPath = themeManager.GetMainMenuBackgroundPath();

            if (deckSelectionMenuPath.Split(".")[^1] == "png")
            {
                mainMenuBackgroundImage.sprite = LoadManager.LoadThemeVisual(deckSelectionMenuPath);
            }
        }
    }
}