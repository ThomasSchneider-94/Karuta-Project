using Karuta.ScriptableObjects;
using System.Collections.Generic;
using Karuta.UIComponent;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using System.Diagnostics.Contracts;

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

        [Header("Type Container Properties")]
        [SerializeField] protected float typeNameScale;
        [SerializeField] private float typeNameWidth;
        [SerializeField] private float typeNameSpacing;

        [Header("Grid properties")]
        [SerializeField] private float nameSpacing = 20;
        [SerializeField] private float nameWidth = 200;
        [Min(1)]
        [SerializeField] private int columnNumber = 3;
        [SerializeField] private Vector2 cellSize = new (100, 100);
        [SerializeField] private Vector2 gridSpacing = new (10, 20);

        [Header("Prefabs")]
        [SerializeField] private Container categoryContainerPrefab;
        [SerializeField] private Container typeContainerPrefab;
        [SerializeField] private CenteredGridLayout gridPrefab;
        [SerializeField] private SelectionButton deckSelectionButtonPrefab;

        [Header("Other")]
        [SerializeField] private List<NumberButton> numberButtons;
        [SerializeField] private Toggle downloadedOnlyToggle;
        [SerializeField] private Button continueButton;


        // Lists
        private readonly List<Container> categoryContainers = new ();
        private int categoriesCount;
        private readonly List<Container> typeContainers = new ();
        private int typesCount;
        private readonly List<CenteredGridLayout> grids = new ();
        private readonly List<SelectionButton> buttons = new ();

        private void OnEnable()
        {
            decksManager = DecksManager.Instance;
            optionsManager = OptionsManager.Instance;

            decksManager.UpdateCategoriesEvent.AddListener(StartUpdateContainers);
            decksManager.UpdateDeckListEvent.AddListener(StartUpdateButtons);
            optionsManager.UpdateCategoryEvent.AddListener(UpdateActiveCategory);
            optionsManager.UpdateMirorMatchEvent.AddListener(RemoveSelectedDoubles);

            downloadedOnlyToggle.SetIsOnWithoutNotify(false);

            continueButton.interactable = false;
            SelectDeckNumber(2);

            // Initialize
            if (decksManager.IsInitialized())
            {
                InitializeContainersAndButtons();
            }
            else
            {
                decksManager.DeckListInitializedEvent.AddListener(InitializeContainersAndButtons);
            }
        }

        public void SelectDeckNumber(int value)
        {
            foreach (NumberButton button in numberButtons)
            {
                button.DeselectButton();
            }
            numberButtons[value - 1].SelectButton();
            decksManager.SetMaxSelectedDeck(value);
        }

        private List<SelectionButton> GetTypeButtons(int category, int type)
        {
            return buttons.GetRange(decksManager.GetTypeIndex(category, type), decksManager.GetTypeDecksCount(category, type));
        }

        #region Create Containers & Buttons
        /// <summary>
        /// Initialize all containers and buttons
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
            categoriesCount = decksManager.GetCategoriesCount();
            typesCount = decksManager.GetTypesCount();
            CreateCategoryContainers();
        }

        /// <summary>
        /// Create all category containers
        /// </summary>
        /// <param name="categoriesDecks"></param>
        private void CreateCategoryContainers()
        {
            for (int i = 0; i < categoriesCount; i++)
            {
                CreateCategoryContainer(i);
            }
        }

        /// <summary>
        /// Create a category container
        /// </summary>
        /// <param name="category"></param>
        private void CreateCategoryContainer(int category)
        {
            Container container = GameObject.Instantiate(categoryContainerPrefab);

            // Add the category container to the list and to it's parent
            categoryContainers.Add(container);

            container.transform.SetParent(allButtonsParent.transform);
            container.transform.localScale = Vector2.one;

            // Create types containers
            CreateTypeContainers(category);

            // Set Parameters
            container.SetAllParameters(decksManager.GetCategory(category), categoryNameScale, categoryNameWidth, categoryNameSpacing, categorySpacing);
        }

        /// <summary>
        /// Create all type containers
        /// </summary>
        /// <param name="category"></param>
        private void CreateTypeContainers(int category)
        {
            for (int i = 0; i < typesCount; i++)
            {
                CreateTypeContainer(category, i);
            }
        }

        /// <summary>
        /// Create a type container
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        private void CreateTypeContainer(int category, int type)
        {
            Container container = GameObject.Instantiate(typeContainerPrefab);

            // Add the type container to the list and it's parent category
            typeContainers.Add(container);
            container.transform.SetParent(categoryContainers[category].GetSubContainer());
            container.transform.localScale = Vector2.one;

            CreateGrid(category, type);

            // Set Parameters
            container.SetAllParameters(decksManager.GetType(type), typeNameScale, typeNameWidth, typeNameSpacing, typeNameSpacing);
        }

        /// <summary>
        /// Create a centered grid to store buttons
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        private void CreateGrid(int category, int type)
        {
            CenteredGridLayout grid = GameObject.Instantiate(gridPrefab);

            // Add the type container to the list and it's parent category
            grids.Add(grid);
            grid.transform.SetParent(typeContainers[category * typesCount + type].GetSubContainer());
            grid.transform.localScale = Vector2.one;

            // Set Parameters
            grid.SetAllParameters(cellSize, gridSpacing, columnNumber);
        }
        #endregion Containers

        #region Buttons
        /// <summary>
        /// Create all the buttons
        /// </summary>
        private void CreateButtons()
        {
            List<DeckInfo> deckList = decksManager.GetDeckList();
            for (int i = 0; i < deckList.Count; i++)
            {
                CreateButton(deckList[i], i);
            }
        }

        /// <summary>
        /// Create a button
        /// </summary>
        /// <param name="deck"></param>
        private void CreateButton(DeckInfo deck, int index)
        {
            SelectionButton button = GameObject.Instantiate(deckSelectionButtonPrefab);

            button.SetDeckName(deck.GetName());
            button.SetNameSpacing(nameSpacing);
            button.SetNameWidth(nameWidth);
            button.SetIconSprite(deck.GetCover());
            button.interactable = true;

            button.onClick.AddListener(delegate { SelectDeck(index); });

            buttons.Add(button);
        }

        /// <summary>
        /// Place the buttons in the containers
        /// </summary>
        private void PlaceButtons()
        {
            for (int i = 0; i < categoriesCount; i++)
            {
                for (int j = 0; j < typesCount; j++)
                {
                    List<SelectionButton> buttonsToAdd = GetTypeButtons(i, j);

                    grids[i * typesCount + j].AddItems(buttonsToAdd);
                    typeContainers[i * typesCount + j].ResizeContainer();

                    foreach (SelectionButton button in buttonsToAdd)
                    {
                        button.transform.localScale = Vector2.one;
                    }
                }

                categoryContainers[i].ResizeContainer();
            }
        }
        #endregion Buttons

        #endregion Create Containers & Buttons

        #region Container & Buttons Update
        /// <summary>
        /// Update the containers list
        /// </summary>
        private void StartUpdateContainers()
        {
            StartCoroutine(UpdateContainers());
        }

        private IEnumerator UpdateContainers()
        {
            yield return StartCoroutine(ClearContainers());

            CreateContainers();

            yield return StartCoroutine(UpdateButtons());
        }

        /// <summary>
        /// Destroy all containers and buttons and clear all lists
        /// </summary>
        private IEnumerator ClearContainers()
        {
            for (int i = 0; i < categoriesCount; i++)
            {
                GameObject.Destroy(categoryContainers[i].gameObject);
            }

            categoryContainers.Clear();
            typeContainers.Clear();
            grids.Clear();
            buttons.Clear();

            yield return null;
        }

        /// <summary>
        /// Update the button list
        /// </summary>
        private void StartUpdateButtons()
        {
            StartCoroutine(UpdateButtons());
        }

        private IEnumerator UpdateButtons()
        {
            yield return StartCoroutine(ClearButtons());

            CreateButtons();

            UpdateActiveButtons();
        }

        /// <summary>
        /// Destroy all buttons and clear button list
        /// </summary>
        private IEnumerator ClearButtons()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                GameObject.Destroy(buttons[i].gameObject);
            }

            buttons.Clear();
            yield return null;
        }
        #endregion Container & Buttons Update

        #region Buttons Gestion
        /// <summary>
        /// Activate the buttons that meets the requirement specified in IsButtonActive
        /// </summary>
        public void UpdateActiveButtons()
        {
            HideNonUsedButtonsAndContainers();

            PlaceButtons();
        }

        /// <summary>
        /// Activate the buttons that meets the requirements and the containers where buttons are active
        /// </summary>
        private void HideNonUsedButtonsAndContainers()
        {
            List<DeckInfo> deckList = decksManager.GetDeckList();

            // Initialize 2 list of bool to false
            List<bool> activeCategories = new(new bool[categoriesCount]);
            List<bool> activeTypes = new(new bool[typeContainers.Count]);

            // Set buttons active
            for (int i = 0; i < deckList.Count; i++)
            {
                if (IsButtonActive(deckList[i]))
                {
                    buttons[i].gameObject.SetActive(true);

                    activeCategories[deckList[i].GetCategory()] = true;
                    activeTypes[deckList[i].GetCategory() * typesCount + deckList[i].GetDeckType()] = true;
                }
                else
                {
                    buttons[i].gameObject.SetActive(false);
                }
            }

            // Set containers active
            for (int i = 0; i < categoriesCount; i++)
            {
                categoryContainers[i].gameObject.SetActive(activeCategories[i] && optionsManager.IsCategoryActive(i));

                for (int j = 0; j < typesCount; j++)
                {
                    typeContainers[i * typesCount + j].gameObject.SetActive(activeTypes[i * typesCount + j]);
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
            return !downloadedOnlyToggle.isOn || deck.IsDownloaded();
        }

        /// <summary>
        /// Called when the active category is changed
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
            for (int i = 0; i < categoriesCount; i++)
            {
                categoryContainers[i].gameObject.SetActive(false);

                for (int j = 0; j < typesCount; j++)
                {
                    typeContainers[i * typesCount + j].gameObject.SetActive(false);

                    // Control if one of the button of the type is active
                    foreach (SelectionButton button in GetTypeButtons(i, j))
                    {
                        if (button.gameObject.activeSelf)
                        {
                            categoryContainers[i].gameObject.SetActive(true);
                            typeContainers[i * typesCount + j].gameObject.SetActive(true);
                            break;
                        }
                    }
                }

                // Set the category active if the deck has made it active and the category is active
                categoryContainers[i].gameObject.SetActive(categoryContainers[i].gameObject.activeSelf && optionsManager.IsCategoryActive(i));
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
                if (!buttons[deckIndex].gameObject.activeSelf || !optionsManager.IsCategoryActive(decksManager.GetDeck(deckIndex).GetCategory()))
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
            // If mirror games are allowded, multiple click on the button will add the same deck
            if (optionsManager.AreMirrorMatchesAllowded())
            {
                // If the list is full, remove all occurences
                if (decksManager.IsSelectedDecksFull())
                {
                    // If the deck was full, make the buttons interactable again
                    SetAllButtonsInteractable(true);
                    
                    decksManager.RemoveSelectedDeck(deckIndex);
                    buttons[deckIndex].DeselectButton();
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

            buttons[deckIndex].SelectButton();

            // If the deck is full, make the buttons non interactable
            if (decksManager.IsSelectedDecksFull())
            {
                SetAllButtonsInteractable(false);
            }
        }

        private void RemoveSelectedDeck(int deckIndex)
        {
            // If the deck was full, make the buttons interactable again
            if (decksManager.IsSelectedDecksFull())
            {
                SetAllButtonsInteractable(true);
            }

            decksManager.RemoveSelectedDeck(deckIndex);
            buttons[deckIndex].DeselectButton();
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
            continueButton.interactable = !interactable;
            List<int> selectedIndexes = decksManager.GetSelectedDecks();
            for (int i = 0; i < buttons.Count; i++)
            {
                if (!selectedIndexes.Contains(i))
                {
                    buttons[i].interactable = interactable;
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
        
        // Called if changes in code or editor (not called at runtime)
        virtual protected void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            allButtonsParent.spacing = globalSpacing;

            foreach (SelectionButton button in buttons)
            {
                button.SetNameWidth(nameWidth);
                button.SetNameSpacing(nameSpacing);
            }

            foreach (CenteredGridLayout grid in grids)
            {
                grid.SetAllParameters(cellSize, gridSpacing, columnNumber);
            }

            foreach (Container container in typeContainers)
            {
                container.SetNameScale(typeNameScale);
                container.SetNameWidth(typeNameWidth);
                container.SetNameSpacing(typeNameSpacing);
            }

            foreach (Container container in categoryContainers)
            {
                container.SetNameScale(categoryNameScale);
                container.SetNameWidth(categoryNameWidth);
                container.SetNameSpacing(categoryNameSpacing);
                container.SetSpacing(categorySpacing);
            }
        }
    }
}
