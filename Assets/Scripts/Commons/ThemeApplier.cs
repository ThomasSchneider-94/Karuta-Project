using UnityEngine;
using UnityEngine.UI;
using Karuta.Objects;
using UnityEngine.Video;
using System.Net;
using System;


namespace Karuta.Commons
{
    public abstract class ThemeApplier : MonoBehaviour
    {
        protected ThemeManager themeManager;
        protected LoadManager loadManager;        

        [SerializeField] protected PanelManager panelManager;
        protected int previousTheme;

        virtual protected void OnEnable()
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

        private void SoftApplyTheme()
        {

        }

        public void ApplyTheme()
        {
            Debug.Log("Apply");

            Debug.Log("Applied");
        }

        protected abstract void ApplyBackground(PanelType panelType);
    }
}