using Karuta.Commons;
using Karuta.Menu;
using Karuta.UIComponent;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Karuta.Game
{
    public class GameThemeApplier : ThemeApplier
    {
        [Header("Game Indications")]
        [SerializeField] private MultiLayerButton notFoundArrow;
        [SerializeField] private MultiLayerButton foundArrow;

        public override void ApplyTheme()
        {
            Debug.Log("Coucou");


            base.ApplyTheme();

            Debug.Log("Coucou");

            ApplyIndicationArrowsColors();
        }

        protected override void ApplyBackground()
        {
            Background background = themeManager.GetGameBackground();

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

        private void ApplyIndicationArrowsColors()
        {
            notFoundArrow.SetColor(0, themeManager.GetNotFoundArrowOutlineColor());
            notFoundArrow.SetColor(1, themeManager.GetNotFoundArrowInsideColor());
            notFoundArrow.GetLabel().color = themeManager.GetNotFoundArrowTextColor();

            foundArrow.SetColor(0, themeManager.GetFoundArrowOutlineColor());
            foundArrow.SetColor(1, themeManager.GetFoundArrowInsideColor());
            foundArrow.GetLabel().color = themeManager.GetFoundArrowTextColor();
        }

    }
}