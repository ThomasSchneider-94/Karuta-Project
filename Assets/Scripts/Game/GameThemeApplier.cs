using System.IO;
using UnityEngine;
using Karuta.Commons;
using Karuta.Objects;
using Karuta.UIComponent;

namespace Karuta.Game
{
    public class GameThemeApplier : ThemeApplier
    {
        [Header("Game Indications")]
        [SerializeField] private MultiLayerButton notFoundArrow;
        [SerializeField] private MultiLayerButton foundArrow;

        #region Theme Applier
        protected override void ApplyTheme(Theme currentTheme, BaseTheme baseTheme)
        {
            base.ApplyTheme(currentTheme, baseTheme);

            ApplyIndicationArrowsColors(currentTheme, baseTheme);
        }

        protected override void ApplyBackground(Theme currentTheme, BaseTheme baseTheme)
        {
            Background background = GetBackgroundFromString(currentTheme.gameBackground, baseTheme.gameBackground);

            if (background.isTexture)
            {
                backgroundRawImage.texture = background.texture;
                videoPlayer.Stop();
            }
            else
            {
                long currentFrame = videoPlayer.frame;
                backgroundRawImage.texture = renderTexture;

                Debug.Log(background.videoPath);

                videoPlayer.url = Path.Combine("file://" + LoadManager.ThemesDirectoryPath, background.videoPath);
                videoPlayer.frame = currentFrame;
                videoPlayer.Play();
            }
        }

        private void ApplyIndicationArrowsColors(Theme currentTheme, BaseTheme baseTheme)
        {
            notFoundArrow.SetColor(0, GetColorFromString(currentTheme.notFoundArrowOutlineColor, baseTheme.notFoundArrowOutlineColor));
            notFoundArrow.SetColor(1, GetColorFromString(currentTheme.notFoundArrowInsideColor, baseTheme.notFoundArrowInsideColor));
            notFoundArrow.GetLabel().color = GetColorFromString(currentTheme.notFoundArrowTextColor, baseTheme.notFoundArrowTextColor);

            foundArrow.SetColor(0, GetColorFromString(currentTheme.foundArrowOutlineColor, baseTheme.foundArrowOutlineColor));
            foundArrow.SetColor(1, GetColorFromString(currentTheme.foundArrowInsideColor, baseTheme.foundArrowInsideColor));
            foundArrow.GetLabel().color = GetColorFromString(currentTheme.foundArrowTextColor, baseTheme.foundArrowTextColor);
        }
        #endregion Theme Applier
    }
}