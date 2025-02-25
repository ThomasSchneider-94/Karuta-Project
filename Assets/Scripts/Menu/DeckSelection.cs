using System.Collections.Generic;
using Karuta.UIComponent;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Karuta.Objects;
using Karuta.UI.CustomButton;
using UnityEngine.Events;
using System.Linq;

namespace Karuta.Menu
{
    public class DeckSelection : MonoBehaviour
    {
        private DecksManager decksManager;
        private OptionsManager optionsManager;

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private VerticalLayoutGroup allButtonsParent;
        [SerializeField] private float globalSpacing;

        [Header("Category Container Properties")]
        [SerializeField] protected float categoryNameScale;
        [SerializeField] private float categoryNameWidth;
        [SerializeField] private float categoryNameSpacing;
        [SerializeField] private float categorySpacing;
        [Range(0f, 1f)]
        [SerializeField] private float categoryOutlineWidth;

        [Header("Type Container Properties")]
        [SerializeField] protected float typeNameScale;
        [SerializeField] private float typeNameWidth;
        [SerializeField] private float typeNameSpacing;
        [Range(0f, 1f)]
        [SerializeField] private float typeOutlineWidth;

        [Header("Grid properties")]
        [SerializeField] private float nameSpacing = 20;
        [SerializeField] private float nameWidth = 200;
        [Min(1)]
        [SerializeField] private int columnNumber = 3;
        [SerializeField] private Vector2 cellSize = new(100, 100);
        [SerializeField] private Vector2 gridSpacing = new(10, 20);

        [Header("Prefabs")]
        [SerializeField] private Container categoryContainerPrefab;
        [SerializeField] private Container typeContainerPrefab;
        [SerializeField] private CenteredGridLayout gridPrefab;
        [SerializeField] private DeckButton deckButtonPrefab;

        [Header("Other")]
        [SerializeField] private Toggle downloadedOnlyToggle;
        [SerializeField] private Button continueButton;

        // Lists
        private readonly List<CenteredGridLayout> grids = new();
        public List<Container> CategoryContainers { get; private set; } = new();
        public List<Container> TypeContainers { get; private set; } = new();
        public List<DeckButton> Buttons { get; private set; } = new();

        public UnityEvent ContainersCreatedEvent { get; } = new();

        public UnityEvent ButtonsCreatedEvent { get; } = new();

        private void Awake()
        {
            decksManager = DecksManager.Instance;
            optionsManager = OptionsManager.Instance;

            decksManager.UpdateCategoriesAndTypesEvent.AddListener(StartUpdateContainers);
            decksManager.UpdateDeckListEvent.AddListener(StartUpdateButtons);
            optionsManager.UpdateCategoryEvent.AddListener(UpdateActiveCategory);
            optionsManager.UpdateMirorMatchEvent.AddListener(RemoveSelectedDoubles);

            downloadedOnlyToggle.SetIsOnWithoutNotify(false);

            continueButton.interactable = false;

            // Initialize
            if (decksManager.IsInitialized())
            {
                InitializeContainersAndButtons();
            }
            else
            {
                decksManager.DeckManagerInitializedEvent.AddListener(InitializeContainersAndButtons);
            }
        }

        #region Create Containers & Buttons
        /// <summary>
        /// Initialize all containers and Buttons
        /// </summary>
        private void InitializeContainersAndButtons()
        {
            CreateContainers();

            CreateButtons();

            HideNonUsedContainers();

            PlaceButtons();

            scrollRect.verticalNormalizedPosition = 1;
        }

        #region Containers
        /// <summary>
        /// Create all containers
        /// </summary>
        private void CreateContainers()
        {
            allButtonsParent.spacing = globalSpacing;
            CreateCategoryContainers();

            ContainersCreatedEvent.Invoke();
        }

        /// <summary>
        /// Create all Category containers
        /// </summary>
        /// <param name="categoriesDecks"></param>
        private void CreateCategoryContainers()
        {
            for (int i = 0; i < decksManager.GetCategoriesCount(); i++)
            {
                CreateCategoryContainer(i);
            }
        }

        /// <summary>
        /// Create a Category container
        /// </summary>
        /// <param name="category"></param>
        private void CreateCategoryContainer(int category)
        {
            Container container = GameObject.Instantiate(categoryContainerPrefab);

            // Add the Category container to the list and to it's parent
            CategoryContainers.Add(container);

            container.transform.SetParent(allButtonsParent.transform);
            container.transform.localScale = Vector2.one;
            container.GetNameTextMesh().outlineWidth = categoryOutlineWidth;

            // Create types containers
            CreateTypeContainers(category);

            // Set Parameters
            container.SetName(decksManager.GetCategoryName(category));
            container.SetNameScale(categoryNameScale);
            container.SetNameWidth(categoryNameWidth);
            container.SetNameSpacing(categoryNameSpacing);
            container.SetSpacing(categorySpacing);
        }

        /// <summary>
        /// Create all CardType containers
        /// </summary>
        /// <param name="category"></param>
        private void CreateTypeContainers(int category)
        {
            for (int i = 0; i < decksManager.GetTypesCount(); i++)
            {
                CreateTypeContainer(category, i);
            }
        }

        /// <summary>
        /// Create a CardType container
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        private void CreateTypeContainer(int category, int type)
        {
            Container container = GameObject.Instantiate(typeContainerPrefab);

            // Add the CardType container to the list and it's parent Category
            TypeContainers.Add(container);
            container.transform.SetParent(CategoryContainers[category].GetSubContainer());
            container.transform.localScale = Vector2.one;
            container.GetNameTextMesh().outlineWidth = typeOutlineWidth;

            CreateGrid(category, type);

            // Set Parameters
            container.SetName(decksManager.GetTypeName(type));
            container.SetNameScale(typeNameScale);
            container.SetNameWidth(typeNameWidth);
            container.SetNameSpacing(typeNameSpacing);
            container.SetSpacing(typeNameSpacing);
        }

        /// <summary>
        /// Create a centered grid to store Buttons
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        private void CreateGrid(int category, int type)
        {
            CenteredGridLayout grid = GameObject.Instantiate(gridPrefab);

            // Add the CardType container to the list and it's parent Category
            grids.Add(grid);
            grid.transform.SetParent(TypeContainers[category * decksManager.GetTypesCount() + type].GetSubContainer());
            grid.transform.localScale = Vector2.one;

            // Set Parameters
            grid.SetCellSize(cellSize);
            grid.SetSpacing(gridSpacing);
            grid.SetColumnNumber(columnNumber);
        }
        #endregion Containers

        #region Buttons
        /// <summary>
        /// Create all the Buttons
        /// </summary>
        private void CreateButtons()
        {
            List<DeckInfo> deckList = decksManager.GetDeckList();
            for (int i = 0; i < deckList.Count; i++)
            {
                CreateButton(deckList[i], i);
            }

            ButtonsCreatedEvent.Invoke();
        }

        /// <summary>
        /// Create a button
        /// </summary>
        /// <param name="deck"></param>
        private void CreateButton(DeckInfo deck, int index)
        {
            DeckButton button = GameObject.Instantiate<DeckButton>(deckButtonPrefab);

            button.SetDeckName(deck.DeckName);
            button.SetNameSpacing(nameSpacing);
            button.SetNameWidth(nameWidth);
            button.SetIconSprite(deck.Cover);
            button.interactable = true;

            button.onClick.AddListener(delegate { SelectDeck(index); });

            Buttons.Add(button);
        }

        /// <summary>
        /// Place the Buttons in the containers
        /// </summary>
        private void PlaceButtons()
        {
            for (int i = 0; i < decksManager.GetCategoriesCount(); i++)
            {
                for (int j = 0; j < decksManager.GetTypesCount(); j++)
                {
                    List<DeckButton> buttonsToAdd = GetTypeButtons(i, j);

                    grids[i * decksManager.GetTypesCount() + j].AddItems(buttonsToAdd);
                    TypeContainers[i * decksManager.GetTypesCount() + j].ForceResize();

                    foreach (DeckButton button in buttonsToAdd)
                    {
                        button.transform.localScale = Vector2.one;
                    }
                }

                CategoryContainers[i].ForceResize();
            }
        }
        #endregion Buttons

        #endregion Create Containers & Buttons



























        #region Container & Buttons Update

        #region Containers Update
        /// <summary>
        /// Update the containers list
        /// </summary>
        private void StartUpdateContainers()
        {
            StartCoroutine(UpdateContainers());
        }

        private IEnumerator UpdateContainers()
        {
            yield return ClearContainers();

            CreateContainers();
        }

        /// <summary>
        /// Destroy all containers and Buttons and clear all lists
        /// </summary>
        private IEnumerator ClearContainers()
        {
            for (int i = 0; i < CategoryContainers.Count; i++)
            {
                GameObject.Destroy(CategoryContainers[i].gameObject);
            }

            CategoryContainers.Clear();
            TypeContainers.Clear();
            grids.Clear();
            Buttons.Clear(); // Clear the Buttons list, they are going to be destroyed anyway

            // yield return to make sure destruction is effective
            yield return null;
        }
        #endregion Containers Update

        #region Buttons Update
        /// <summary>
        /// Update the button list
        /// </summary>
        private void StartUpdateButtons()
        {
            StartCoroutine(UpdateButtons());
        }

        private IEnumerator UpdateButtons()
        {
            yield return ClearButtons();

            CreateButtons();

            UpdateActiveButtons();
        }

        /// <summary>
        /// Destroy all Buttons and clear button list
        /// </summary>
        private IEnumerator ClearButtons()
        {
            for (int i = 0; i < Buttons.Count; i++)
            {
                GameObject.Destroy(Buttons[i].gameObject);
            }

            Buttons.Clear();
            yield return null;
        }
        #endregion Buttons Update

        #endregion Container & Buttons Update

        #region Buttons Gestion
        /// <summary>
        /// Activate the Buttons that meets the requirement specified in IsButtonActive
        /// </summary>
        public void UpdateActiveButtons()
        {
            HideNonUsedButtonsAndContainers();

            PlaceButtons();
        }

        /// <summary>
        /// Activate the Buttons that meets the requirements and the containers where Buttons are active
        /// </summary>
        private void HideNonUsedButtonsAndContainers()
        {
            List<DeckInfo> deckList = decksManager.GetDeckList();

            // Initialize 2 list of bool to false
            List<bool> activeCategories = new(new bool[CategoryContainers.Count]);
            List<bool> activeTypes = new(new bool[TypeContainers.Count]);

            // Set Buttons active
            for (int i = 0; i < deckList.Count; i++)
            {
                if (IsButtonActive(deckList[i]))
                {
                    Buttons[i].gameObject.SetActive(true);

                    activeCategories[deckList[i].Category] = true;
                    activeTypes[deckList[i].Category * decksManager.GetTypesCount() + deckList[i].DeckType] = true;
                }
                else
                {
                    Buttons[i].gameObject.SetActive(false);
                }
            }

            // Set containers active
            for (int i = 0; i < decksManager.GetCategoriesCount(); i++)
            {
                CategoryContainers[i].gameObject.SetActive(activeCategories[i] && optionsManager.IsCategoryActive(i));

                for (int j = 0; j < decksManager.GetTypesCount(); j++)
                {
                    TypeContainers[i * decksManager.GetTypesCount() + j].gameObject.SetActive(activeTypes[i * decksManager.GetTypesCount() + j]);
                }
            }

            RemoveNonActiveSelectedDecks();
        }

        /// <summary>
        /// Specify if a button is active
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        private bool IsButtonActive(DeckInfo deck)
        {
            return !downloadedOnlyToggle.isOn || deck.IsDownloaded;
        }

        private List<int> GetActiveButtonIndexes()
        {
            return decksManager.GetDeckList()
                .Select((deck, index) => (deck, index))
                    .Where(item => IsButtonActive(item.deck))
                        .Select(item => item.index)
            .ToList();
        }

        private List<int> GetNonActiveButtonIndexes()
        {
            return decksManager.GetDeckList()
                .Select((deck, index) => (deck, index))
                    .Where(item => !IsButtonActive(item.deck))
                        .Select(item => item.index)
            .ToList();
        }

        /// <summary>
        /// Called when the active Category is changed
        /// </summary>
        private void UpdateActiveCategory()
        {
            HideNonUsedContainers();

            StartCoroutine(ResetScrollPosition());
        }

        /// <summary>
        /// Hide the containers where no button is active
        /// </summary>
        private void HideNonUsedContainers()
        {
            for (int i = 0; i < decksManager.GetCategoriesCount(); i++)
            {
                CategoryContainers[i].gameObject.SetActive(false);

                for (int j = 0; j < decksManager.GetTypesCount(); j++)
                {
                    TypeContainers[i * decksManager.GetTypesCount() + j].gameObject.SetActive(false);

                    // Control if one of the button of the CardType is active
                    foreach (SelectionButton button in GetTypeButtons(i, j))
                    {
                        if (button.gameObject.activeSelf)
                        {
                            CategoryContainers[i].gameObject.SetActive(true);
                            TypeContainers[i * decksManager.GetTypesCount() + j].gameObject.SetActive(true);
                            break;
                        }
                    }
                }

                // Set the Category active if the Deck has made it active and the Category is active
                CategoryContainers[i].gameObject.SetActive(CategoryContainers[i].gameObject.activeSelf && optionsManager.IsCategoryActive(i));
            }
            RemoveNonActiveSelectedDecks();
        }

        /// <summary>
        /// If a button does not appear on screen, remove it from the selected decks list
        /// </summary>
        private void RemoveNonActiveSelectedDecks()
        {
            List<int> indexesToRemove = new();
            foreach (int deckIndex in decksManager.GetSelectedDecks())
            {
                if (!Buttons[deckIndex].gameObject.activeSelf || !optionsManager.IsCategoryActive(decksManager.GetDeck(deckIndex).Category))
                {
                    indexesToRemove.Add(deckIndex);
                }
            }

            foreach (int deckIndex in indexesToRemove)
            {
                RemoveSelectedDeck(deckIndex);
            }
        }
        #endregion Buttons Gestion

        #region Button Function
        public void SelectDeck(int deckIndex)
        {
            // If mirror games are allowded, multiple click on the button will add the same Deck
            if (optionsManager.AreMirrorMatchesAllowded())
            {
                // If the list is full, remove all occurences
                if (decksManager.IsSelectedDecksFull())
                {
                    // If the Deck was full, make the Buttons interactable again
                    SetAllButtonsInteractable(true);
                    
                    decksManager.RemoveSelectedDeck(deckIndex);
                    Buttons[deckIndex].DeselectButton();
                }
                else
                {
                    // Else, add it to the selected decks
                    AddSelectedDeck(deckIndex);
                }
            }
            // If mirror game are note allowded
            else
            {
                // If index already in selected, remove it
                if (decksManager.GetSelectedDecks().Contains(deckIndex))
                {
                    RemoveSelectedDeck(deckIndex);
                }
                // If not, add it
                else
                {
                    AddSelectedDeck(deckIndex);
                }
            }
        }

        private void AddSelectedDeck(int deckIndex)
        {
            decksManager.AddSelectedDeck(deckIndex);

            Buttons[deckIndex].SelectButton();

            // If the Deck is full, make the Buttons non interactable
            if (decksManager.IsSelectedDecksFull())
            {
                SetAllButtonsInteractable(false);
            }
        }

        private void RemoveSelectedDeck(int deckIndex)
        {
            // If the Deck was full, make the Buttons interactable again
            if (decksManager.IsSelectedDecksFull())
            {
                SetAllButtonsInteractable(true);
            }

            decksManager.RemoveSelectedDeck(deckIndex);
            Buttons[deckIndex].DeselectButton();
        }

        public void RemoveAllSelectedDeck()
        {
            List<int> selected = new();
            foreach(int deckIndex in decksManager.GetSelectedDecks())
            {
                selected.Add(deckIndex);
            }

            foreach (int deckIndex in selected) 
            {
                RemoveSelectedDeck(deckIndex);
            }
        }

        private void SetAllButtonsInteractable(bool interactable)
        {
            Debug.Log("Set all interactable " + interactable);
            continueButton.interactable = !interactable;
            List<int> selectedIndexes = decksManager.GetSelectedDecks();
            for (int i = 0; i < Buttons.Count; i++)
            {
                if (!selectedIndexes.Contains(i))
                {
                    Buttons[i].interactable = interactable;
                }
            }
        }

        private void RemoveSelectedDoubles()
        {
            List<int> indexes = new();
            foreach (int deckIndex in decksManager.GetSelectedDecks())
            {
                indexes.Add(deckIndex);
            }

            foreach (int deckIndex in indexes)
            {
                RemoveSelectedDeck(deckIndex);
                AddSelectedDeck(deckIndex);
            }
        }
        #endregion Button Function

        private IEnumerator ResetScrollPosition()
        {
            yield return null;  // Wait one frame
            scrollRect.verticalNormalizedPosition = 1;
        }

        private List<DeckButton> GetTypeButtons(int category, int type)
        {
            return Buttons.GetRange(decksManager.GetTypeIndex(category, type), decksManager.GetTypeDecksCount(category, type));
        }

#if UNITY_EDITOR
        // Called if changes in code or editor (not called at runtime)
        virtual protected void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            allButtonsParent.spacing = globalSpacing;

            foreach (SelectionButton button in Buttons)
            {
                button.SetNameWidth(nameWidth);
                button.SetNameSpacing(nameSpacing);
            }

            foreach (CenteredGridLayout grid in grids)
            {
                grid.SetCellSize(cellSize);
                grid.SetSpacing(gridSpacing);
                grid.SetColumnNumber(columnNumber);
            }

            foreach (Container container in TypeContainers)
            {
                container.SetNameScale(typeNameScale);
                container.SetNameWidth(typeNameWidth);
                container.SetNameSpacing(typeNameSpacing);
                container.GetNameTextMesh().outlineWidth = typeOutlineWidth;
            }

            foreach (Container container in CategoryContainers)
            {
                container.SetNameScale(categoryNameScale);
                container.SetNameWidth(categoryNameWidth);
                container.SetNameSpacing(categoryNameSpacing);
                container.SetSpacing(categorySpacing);
                container.GetNameTextMesh().outlineWidth = categoryOutlineWidth;
            }
        }
#endif
    }
}
