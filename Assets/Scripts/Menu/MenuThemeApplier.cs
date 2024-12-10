using Karuta.Commons;
using Karuta.Objects;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Karuta.Menu
{
    public class MenuThemeApplier : ThemeApplier
    {
        [Header("Background")]
        [SerializeField] private RawImage backgroundRawImage;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RenderTexture renderTexture;

        override protected void OnEnable()
        {
            panelManager.PanelUpdateEvent.AddListener(ApplyBackground);

            base.OnEnable();
        }

        protected override void ApplyBackground(PanelType panelType)
        {
            if (panelType == PanelType.None) { return; }

            Debug.Log(panelType);

            // Switch for a single variable
            Background background = panelType switch
            {
                PanelType.MainMenu => themeManager.GetMainMenuBackground(),
                PanelType.DeckSelection => themeManager.GetDecksChoiceBackground(),
                PanelType.Game => themeManager.GetGameBackground(),
                _ => themeManager.GetMainMenuBackground(),
            };

            if (background.isVideo)
            {
                long currentFrame = videoPlayer.frame;
                backgroundRawImage.texture = renderTexture;
                videoPlayer.url = Path.Combine("file://" + LoadManager.ThemesDirectoryPath, background.videoPath);
                videoPlayer.frame = currentFrame;
                videoPlayer.Play();
            }
            else
            {
                backgroundRawImage.texture = background.texture;
                videoPlayer.Stop();
            }
        }
    }
}