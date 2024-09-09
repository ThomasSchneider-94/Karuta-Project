using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Karuta
{
    public class Menu : MonoBehaviour
    {
        private GameManager gameManager;

        [Header("Panels")]
        [SerializeField] private GameObject optionsPanel;
        private bool optionsPanelActive;
        [SerializeField] private GameObject decksDownloadPanel;
        private bool decksDownloadPanelActive;

        // Start is called before the first frame update
        void Start()
        {
            gameManager = GameManager.Instance;

            SetOptionsPanel(false);
            SetDecksDownloadPanel(false);
        }

        // Hide or Show Panels
        #region Hide / Show panels
        public void SwitchOptionsPanel()
        {
            optionsPanelActive = !optionsPanelActive;
            optionsPanel.SetActive(optionsPanelActive);
        }
        public void SetOptionsPanel(bool isActive)
        {
            optionsPanelActive = isActive;
            optionsPanel.SetActive(optionsPanelActive);
        }
        public void SwitchDecksDownloadPanel()
        {
            decksDownloadPanelActive = !decksDownloadPanelActive;
            decksDownloadPanel.SetActive(decksDownloadPanelActive);
            SwitchOptionsPanel();
        }

        public void SetDecksDownloadPanel(bool isActive)
        {
            decksDownloadPanelActive = isActive;
            decksDownloadPanel.SetActive(decksDownloadPanelActive);
        }
        #endregion Hide / Show panels
    }
}
