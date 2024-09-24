using Karuta;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Karuta.Menu
{
    public class PanelManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject basePanel;
        [SerializeField] private GameObject deckSelectionPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject decksDownloadPanel;

        // Start is called before the first frame update
        private void Start()
        {
            basePanel.SetActive(true);
            deckSelectionPanel.SetActive(false);
            optionsPanel.SetActive(false);
            decksDownloadPanel.SetActive(false);
        }
        
        #region Base and Deck Selection Panel
        public void SwitchBaseAndDeckSelectionPanel()
        {
            basePanel.SetActive(!basePanel.activeSelf);
            deckSelectionPanel.SetActive(!deckSelectionPanel.activeSelf);
        }

        public void SetBasePanel(bool isActive)
        {
            basePanel.SetActive(isActive);

            deckSelectionPanel.SetActive(!isActive);
        }

        public void SetDeckSelection(bool isActive)
        {
            deckSelectionPanel.SetActive(isActive);

            basePanel.SetActive(!isActive);
        }
        #endregion Base and Deck Selection Panel

        #region Option Panel
        public void SwitchOptionsPanel()
        {
            optionsPanel.SetActive(!optionsPanel.activeSelf);
        }
        #endregion Option Panel

        #region Deck Download Panel
        public void SwitchDecksDownloadPanel()
        {
            decksDownloadPanel.SetActive(!decksDownloadPanel.activeSelf);
            SwitchOptionsPanel();
        }

        public void SetDecksDownloadPanel(bool isActive)
        {
            decksDownloadPanel.SetActive(isActive);
            if (isActive)
            {
                optionsPanel.SetActive(false);
            }
        }
        #endregion Deck Download Panel
    }
}