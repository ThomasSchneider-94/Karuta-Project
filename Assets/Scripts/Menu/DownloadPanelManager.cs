using Karuta.Objects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using Karuta.UI.CustomButton;
using Karuta.UI;

namespace Karuta.Menu
{
    public class DownloadPanelManager : MonoBehaviour
    {
        [Header("Toggle General")]
        [SerializeField] private LabeledToggle togglePrefab;
        [SerializeField] private Toggle selectAllToggle;

        [Header("Toggle Layout")]
        [SerializeField] private VerticalLayoutGroup togglesParent;
        [SerializeField] private float toggleSpacing;
        [SerializeField] private Vector2 toggleScale;

        [Header("Delete Mode Panel")]
        [SerializeField] private Image panel;
        [SerializeField] private Color panelColorOnDelete;

        [Header("Delete Mode Buttons")]
        [SerializeField] private Button downloadDeckButton;
        [SerializeField] private Button deleteDeckButton;
        [SerializeField] private ColorFadeButton enableDeleteButton;

        public List<LabeledToggle> Toggles { get; } = new();
        private bool deleteModeOn = false;

        public UnityEvent TogglesCreatedEvent { get; } = new();

        protected void Awake()
        {
            DecksManager.Instance.UpdateDeckListEvent.AddListener(UpdateToggles);
            OptionsManager.Instance.UpdateCategoryEvent.AddListener(HideNonUsedToggles);

            downloadDeckButton.interactable = false;
            deleteDeckButton.interactable = false;

            selectAllToggle.SetIsOnWithoutNotify(false);

            togglesParent.spacing = toggleSpacing;
            togglesParent.transform.localScale = toggleScale;

            // Initialize
            if (DecksManager.Instance.IsInitialized())
            {
                CreateToggles();
            }
            else
            {
                DecksManager.Instance.DeckManagerInitializedEvent.AddListener(CreateToggles);
            }
        }

        #region Toggles Creation
        /// <summary>
        /// Update the decks download Toggles
        /// </summary>
        private void UpdateToggles()
        {
            foreach (LabeledToggle toggle in Toggles)
            {
                GameObject.Destroy(toggle.gameObject);
            }
            Toggles.Clear();

            CreateToggles();
        }

        /// <summary>
        /// Create all the Toggles to download decks
        /// </summary>
        private void CreateToggles()
        {
            foreach (string deckName in DecksManager.Instance.GetDeckList().Select(deck => deck.DeckName))
            {
                CreateToggle(deckName);
            }
            HideNonUsedToggles();

            TogglesCreatedEvent.Invoke();
        }

        /// <summary>
        /// Create a toggle to download a Deck
        /// </summary>
        /// <param name="deckName"></param>
        private void CreateToggle(string deckName)
        {
            LabeledToggle toggle = GameObject.Instantiate(togglePrefab);

            toggle.transform.SetParent(togglesParent.transform); // setting parent

            // Set Toggle Values
            toggle.SetLabel(deckName);
            toggle.SetIsOnWithoutNotify(false);
            toggle.transform.localScale = Vector2.one;

            toggle.onValueChanged.AddListener(delegate { SetButtonsAndToggle(); });

            Toggles.Add(toggle);
        }
        #endregion Toggles Creation

        #region Toggles Gestion
        /// <summary>
        /// Hide or Show the Deck toggle. Hide if the Deck is already downloaded or its Category is not the current one
        /// </summary>
        private void HideNonUsedToggles()
        {
            List<DeckInfo> deckList = DecksManager.Instance.GetDeckList();

            for (int i = 0; i < deckList.Count; i++)
            {
                // If a Deck is downloaded, make it appears when deletingModeOn is true
                if (deleteModeOn == deckList[i].IsDownloaded && OptionsManager.Instance.IsCategoryActive(deckList[i].Category))
                {
                    Toggles[i].gameObject.SetActive(true);
                }
                else
                {
                    Toggles[i].gameObject.SetActive(false);
                    Toggles[i].SetIsOnWithoutNotify(false);
                }
            }

            SetButtonsAndToggle();
        }

        /// <summary>
        /// Ckeck if one or all active Toggles are on
        /// </summary>
        private void SetButtonsAndToggle()
        {
            bool one = false;
            bool all = true;

            foreach (LabeledToggle toggle in Toggles)
            {
                if (toggle.gameObject.activeSelf)
                {
                    if (!one && toggle.isOn)
                    {
                        // If one is found to be checked
                        one = true;
                    }
                    else if (all && !toggle.isOn)
                    {
                        // If one is not checked
                        all = false;
                    }
                    else
                    {
                        // If one is found checked and one not checked
                        break;
                    }
                }
            }

            selectAllToggle.SetIsOnWithoutNotify(all);
            downloadDeckButton.interactable = one;
            deleteDeckButton.interactable = one;
        }

        /// <summary>
        /// Set all the active Toggles to the value of the SelectAll toggle
        /// </summary>
        public void SetAllToggleIsOn()
        {
            bool isOn = selectAllToggle.isOn;

            foreach (LabeledToggle toggle in Toggles)
            {
                if (toggle.gameObject.activeSelf)
                {
                    toggle.SetIsOnWithoutNotify(isOn);
                }
            }
            downloadDeckButton.interactable = isOn;
            deleteDeckButton.interactable = isOn;
        }
        #endregion Toggles Gestion

        #region Delete Mode
        /// <summary>
        /// Switch between delete on download mode
        /// </summary>
        public void SwitchToDeleteMode()
        {
            deleteModeOn = !deleteModeOn;
            enableDeleteButton.SelectButton(deleteModeOn);

            panel.color = deleteModeOn ? new(panelColorOnDelete.r, panelColorOnDelete.g, panelColorOnDelete.b, panel.color.a) : new(1, 1, 1, panel.color.a);

            downloadDeckButton.gameObject.SetActive(!deleteModeOn);
            deleteDeckButton.gameObject.SetActive(deleteModeOn);
            HideNonUsedToggles();
        }

        /// <summary>
        /// Disable Delete mode
        /// </summary>
        public void DisableDeleteModeOnClose()
        {
            if (deleteModeOn)
            {
                SwitchToDeleteMode();
            }
        }
        #endregion Delete Mode

        public List<int> GetOnTogglesIndexes()
        {
            return Toggles
                .Select((toggle, index) => (toggle, index))
                    .Where(item => item.toggle.isOn)
                        .Select(item => item.index)
            .ToList();
        }

        public List<int> GetOffTogglesIndexes()
        {
            return Toggles
                .Select((toggle, index) => (toggle, index))
                    .Where(item => !item.toggle.isOn)
                        .Select(item => item.index)
            .ToList();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (togglesParent != null)
            {
                togglesParent.spacing = toggleSpacing;
                togglesParent.transform.localScale = toggleScale;
            }
        }
#endif
    }
}