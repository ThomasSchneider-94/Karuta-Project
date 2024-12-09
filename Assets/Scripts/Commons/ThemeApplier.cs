using UnityEngine;
using UnityEngine.UI;
using Karuta.Objects;
using UnityEngine.Video;
using System.Net;
using System;


namespace Karuta.Commons
{
    public class ThemeApplier : MonoBehaviour
    {
        ThemeManager themeManager;
        LoadManager loadManager;

        [SerializeField] private bool inGame;

        [Header("Backgrounds")]
        [SerializeField] private Image imageBackGround;
        [SerializeField] private VideoPlayer videoBackGround;


        private int previousTheme;




        private void OnEnable()
        {
            themeManager = ThemeManager.Instance;
            loadManager = LoadManager.Instance;

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
            Debug.Log("Apply");

            Theme currentTheme = themeManager.GetCurrentTheme();

            ApplyBackgrounds(currentTheme);

            Debug.Log("Applied");
        }




        private void ApplyBackgrounds(Theme theme)
        {
            if (inGame)
            {
                if (theme.GetGameBackground().Split(".")[^1] == "png")
                {
                    loadManager.Load
                }




            }






            string[] names = theme.GetMainMenuBackground().Split(".");
















        }








        private void SoftApplyTheme()
        {

        }
    }
}