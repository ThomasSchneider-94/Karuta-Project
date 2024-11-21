using Karuta;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Karuta.Commons
{
    public class PanelManager : MonoBehaviour
    {
        [SerializeField] private GameObject startPanel;


        [Header("Panels")]
        [SerializeField] private List<GameObject> panels = new();
        
        private readonly Stack<GameObject> currentPanels = new();

        private void Start()
        {
            TogglePanel(startPanel);
        }

        public void TogglePanel(GameObject panelToToggle)
        {
            currentPanels.Push(panelToToggle);

            foreach (GameObject panel in panels)
            {
                panel.SetActive(panel == panelToToggle);
            }
        }

        public void ReturnToPreviousPanel()
        {
            if (currentPanels.Count <= 1) { return; }

            currentPanels.Pop();

            foreach (GameObject panel in panels)
            {
                panel.SetActive(panel == currentPanels.Peek());
            }
        }

        public void ReturnToPreviousPanel(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                ReturnToPreviousPanel();
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