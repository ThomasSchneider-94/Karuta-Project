using Karuta;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        DeckSelection
    }

    [System.Serializable]
    public class Panel
    {
        public GameObject panelObject;
        public PanelType panelType;
        public List<GameObject> allowedPanelObject;
    }

    public class PanelManager : MonoBehaviour
    {
        [SerializeField] private GameObject startPanel;

        [Header("Panels")]
        [SerializeField] private List<Panel> panels = new();

        public UnityEvent<PanelType> PanelUpdateEvent { get; } = new();

        private readonly Stack<Panel> currentPanels = new();

        private void Start()
        {
            TogglePanel(startPanel);
        }

        public void TogglePanel(GameObject panelObjectToToggle)
        {
            Panel panelToToggle = new();
            foreach (Panel panel in panels)
            {
                if (panel.panelObject == panelObjectToToggle)
                {
                    panelToToggle = panel;
                    break;
                }
            }

            // Desactivate the panel not allowded
            foreach (var panel in panels.Where(panel => !panelToToggle.allowedPanelObject.Contains(panel.panelObject)))
            {
                panel.panelObject.SetActive(false);
            }

            currentPanels.Push(panelToToggle);
            panelToToggle.panelObject.SetActive(true);
            PanelUpdateEvent.Invoke(panelToToggle.panelType);
        }

        public void ReturnToPreviousPanel()
        {
            if (currentPanels.Count <= 1) { return; }

            currentPanels.Pop();

            Panel panelToToggle = currentPanels.Peek();

            // Desactivate the panel not allowded
            foreach (var panel in panels.Where(panel => !panelToToggle.allowedPanelObject.Contains(panel.panelObject)))
            {
                panel.panelObject.SetActive(false);
            }

            panelToToggle.panelObject.SetActive(true);
            PanelUpdateEvent.Invoke(panelToToggle.panelType);
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