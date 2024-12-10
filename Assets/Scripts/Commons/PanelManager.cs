using Karuta;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Karuta.Commons
{
    public enum PanelType
    {
        None,
        MainMenu,
        DeckSelection,
        Game
    }

    [System.Serializable]
    public class Panel
    {
        public GameObject panel;
        public PanelType type;
    }

    public class PanelManager : MonoBehaviour
    {
        [SerializeField] private Panel startPanel;

        [Header("Panels")]
        [SerializeField] private List<Panel> panels = new();

        public UnityEvent<PanelType> PanelUpdateEvent { get; } = new();

        private readonly Stack<Panel> currentPanels = new();

        private void Start()
        {
            TogglePanel(startPanel.panel);
        }

        public void TogglePanel(GameObject panelToToggle)
        {
            foreach (Panel panel in panels)
            {
                if (panel.panel == panelToToggle)
                {
                    currentPanels.Push(panel);
                    panel.panel.SetActive(true);
                    PanelUpdateEvent.Invoke(panel.type);
                }
                else
                {
                    panel.panel.SetActive(false);
                }
            }
        }

        public void ReturnToPreviousPanel()
        {
            if (currentPanels.Count <= 1) { return; }

            currentPanels.Pop();

            foreach (Panel panel in panels)
            {
                if (panel == currentPanels.Peek())
                {
                    panel.panel.SetActive(true);
                    PanelUpdateEvent.Invoke(panel.type);
                }
                else
                {
                    panel.panel.SetActive(false);
                }
            }
        }

        public static void LoadMenu()
        {
            SceneManager.LoadScene("Menu");
        }

        public static void LoadGame()
        {
            SceneManager.LoadScene("Game");
        }
    }
}