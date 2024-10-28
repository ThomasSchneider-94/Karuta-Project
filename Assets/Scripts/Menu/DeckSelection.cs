using Karuta.ScriptableObjects;
using System.Collections.Generic;
using Karuta.UIComponent;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Karuta.Menu
{
    public class DeckSelection : MonoBehaviour
    {
        private GameManager gameManager;

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private VerticalLayoutGroup allButtonsParent;
        [SerializeField] private float globalSpacing;
        [SerializeField] private Toggle downloadedOnlyToggle;

        [Header("Category Container Properties")]
        [SerializeField] protected float categoryNameScale;
        [SerializeField] private float categoryNameWidth;
        [SerializeField] private float categorySpacing;

        [Header("Type Container Properties")]
        [SerializeField] protected float typeNameScale;
        [SerializeField] private float typeNameWidth;
        [SerializeField] private float typeSpacing;

        [Header("Grid properties")]
        [Min(1)]
        [SerializeField] private int columnNumber = 3;
        [SerializeField] private Vector2 cellSize = new (100, 100);
        [SerializeField] private Vector2 gridSpacing = new (10, 20);

        [Header("Prefabs")]
        [SerializeField] private Container categoryContainerPrefab;
        [SerializeField] private Container typeContainerPrefab;
        [SerializeField] private CenteredGridLayout gridPrefab;
        [SerializeField] private SelectionButton selectionButtonPrefab;

        // Lists
        private readonly List<Container> categoryContainers = new ();
        private readonly List<Container> typeContainers = new ();
        private readonly List<CenteredGridLayout> grids = new ();
        private readonly List<SelectionButton> buttons = new ();

        private void Awake()
        {
            gameManager = GameManager.Instance;

            gameManager.InitializeDeckListEvent.AddListener(InitializeContainersAndButtons);
            gameManager.UpdateDeckListEvent.AddListener(UpdateButtons);
            gameManager.UpdateCategoryEvent.AddListener(UpdateActiveCategory);
            gameManager.UpdateMirorMatchEvent.AddListener(RemoveSelectedDoubles);
            //Debug.Log("Selection Subscription")

            downloadedOnlyToggle.SetIsOnWithoutNotify(false);
        }

        #region Get Buttons
        private List<SelectionButton> GetCategoryButtons(DeckInfo.DeckCategory category)
        {
            return buttons.GetRange(gameManager.GetCategoryIndex(category), gameManager.GetCategoryCount(category));
        }

        private List<SelectionButton> GetTypeButtons(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            return buttons.GetRange(gameManager.GetTypeIndex(category, type), gameManager.GetTypeCount(category, type));
        }
        #endregion Get Buttons

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
            CreateCategoryContainers();
        }

        /// <summary>
        /// Create all category containers
        /// </summary>
        /// <param name="categoriesDecks"></param>
        private void CreateCategoryContainers()
        {
            for (int i = 0; i < (int)DeckInfo.DeckCategory.CATEGORY_NB; i++)
            {
                CreateCategoryContainer((DeckInfo.DeckCategory)i);
            }
        }

        /// <summary>
        /// Create a category container
        /// </summary>
        /// <param name="category"></param>
        private void CreateCategoryContainer(DeckInfo.DeckCategory category)
        {
            Container container = GameObject.Instantiate(categoryContainerPrefab);

            // Add the category container to the list and to it's parent
            categoryContainers.Add(container);
            container.transform.SetParent(allButtonsParent.transform);

            // Set Parameters
            container.SetAllParameters(category.ToString(), categoryNameScale, categoryNameWidth, categorySpacing);

            // Create types containers
            CreateTypeContainers(category);

            container.FindChildRectTransforms();
        }

        /// <summary>
        /// Create all type containers
        /// </summary>
        /// <param name="category"></param>
        private void CreateTypeContainers(DeckInfo.DeckCategory category)
        {
            for (int i = 0; i < (int)DeckInfo.DeckType.TYPE_NB; i++)
            {
                CreateTypeContainer(category, (DeckInfo.DeckType)i);
            }
        }

        /// <summary>
        /// Create a type container
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        private void CreateTypeContainer(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            Container container = GameObject.Instantiate(typeContainerPrefab);

            // Add the type container to the list and it's parent category
            typeContainers.Add(container);
            container.transform.SetParent(categoryContainers[(int)category].transform);

            // Set Parameters
            container.SetAllParameters(type.ToString(), typeNameScale, typeNameWidth, typeSpacing);

            CreateGrid(category, type);

            container.FindChildRectTransforms();
        }

        /// <summary>
        /// Create a centered grid to store buttons
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        private void CreateGrid(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            CenteredGridLayout grid = GameObject.Instantiate(gridPrefab);

            // Add the type container to the list and it's parent category
            grids.Add(grid);
            grid.transform.SetParent(typeContainers[(int)category * (int)DeckInfo.DeckType.TYPE_NB + (int)type].transform);

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
            List<DeckInfo> deckList = gameManager.GetDeckList();
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
            SelectionButton button = GameObject.Instantiate(selectionButtonPrefab);

            button.SetDeckName(deck.GetName());
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
            for (int i = 0; i < (int)DeckInfo.DeckCategory.CATEGORY_NB; i++)
            {
                for (int j = 0; j < (int)DeckInfo.DeckType.TYPE_NB; j++)
                {
                    grids[i * (int)DeckInfo.DeckType.TYPE_NB + j].AddItems(GetTypeButtons((DeckInfo.DeckCategory)i, (DeckInfo.DeckType)j));
                    typeContainers[i * (int)DeckInfo.DeckType.TYPE_NB + j].ResizeContainer();
                }

                categoryContainers[i].ResizeContainer();
            }
        }
        #endregion Buttons

        #endregion Create Containers & Buttons

        #region Buttons Gestion
        /// <summary>
        /// Update the button list
        /// </summary>
        private void UpdateButtons()
        {
            ClearButtons();

            CreateButtons();

            UpdateActiveButtons();
        }

        /// <summary>
        /// Activate the buttons that meets the requirement specified in IsButtonActive
        /// </summary>
        public void UpdateActiveButtons()
        {
            HideNonUsedButtonsAndContainers();

            PlaceButtons();
        }

        /// <summary>
        /// Destroy all buttons and clear button list
        /// </summary>
        private void ClearButtons()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                GameObject.Destroy(buttons[i].gameObject);
            }

            buttons.Clear();
        }

        /// <summary>
        /// Activate the buttons that meets the requirements and the containers where buttons are active
        /// </summary>
        private void HideNonUsedButtonsAndContainers()
        {
            List<DeckInfo> deckList = gameManager.GetDeckList();

            // Initialize 2 list of bool to false
            List<bool> activeCategories = new(new bool[(int)DeckInfo.DeckCategory.CATEGORY_NB]);
            List<bool> activeTypes = new(new bool[(int)DeckInfo.DeckCategory.CATEGORY_NB * (int)DeckInfo.DeckType.TYPE_NB]);

            // Set buttons active
            for (int i = 0; i < deckList.Count; i++)
            {
                if (IsButtonActive(deckList[i]))
                {
                    buttons[i].gameObject.SetActive(true);

                    activeCategories[(int)deckList[i].GetCategory()] = true;
                    activeTypes[(int)deckList[i].GetCategory() * (int)DeckInfo.DeckType.TYPE_NB + (int)deckList[i].GetDeckType()] = true;
                }
                else
                {
                    buttons[i].gameObject.SetActive(false);
                }
            }

            // Set containers active
            for (int i = 0; i < (int)DeckInfo.DeckCategory.CATEGORY_NB; i++)
            {
                categoryContainers[i].gameObject.SetActive(activeCategories[i] && gameManager.IsCategoryActive((DeckInfo.DeckCategory)i));

                for (int j = 0; j < (int)DeckInfo.DeckType.TYPE_NB; j++)
                {
                    typeContainers[i * (int)DeckInfo.DeckType.TYPE_NB + j].gameObject.SetActive(activeTypes[i * (int)DeckInfo.DeckType.TYPE_NB + j]);
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
            for (int i = 0; i < (int)DeckInfo.DeckCategory.CATEGORY_NB; i++)
            {
                categoryContainers[i].gameObject.SetActive(false);

                for (int j = 0; j < (int)DeckInfo.DeckType.TYPE_NB; j++)
                {
                    typeContainers[i * (int)DeckInfo.DeckType.TYPE_NB + j].gameObject.SetActive(false);

                    // Control if one of the button of the type is active
                    foreach (SelectionButton button in GetTypeButtons((DeckInfo.DeckCategory)i, (DeckInfo.DeckType)j))
                    {
                        if (button.gameObject.activeSelf)
                        {
                            categoryContainers[i].gameObject.SetActive(true);
                            typeContainers[i * (int)DeckInfo.DeckType.TYPE_NB + j].gameObject.SetActive(true);
                            break;
                        }
                    }
                }

                // Set the category active if the deck has made it active and the category is active
                categoryContainers[i].gameObject.SetActive(categoryContainers[i].gameObject.activeSelf && gameManager.IsCategoryActive((DeckInfo.DeckCategory)i));
            }
            RemoveNonActiveSelectedDecks();
        }

        /// <summary>
        /// If a button does not appear on screen, remove it from the selectioned decks list
        /// </summary>
        private void RemoveNonActiveSelectedDecks()
        {
            List<int> indexesToRemove = new();
            foreach (int deckIndex in gameManager.GetSelectedDecks())
            {
                if (!buttons[deckIndex].gameObject.activeSelf || !gameManager.IsCategoryActive(gameManager.GetDeck(deckIndex).GetCategory()))
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
            if (gameManager.AreMirrorMatchesAllowded())
            {
                // If the list is full, remove all occurences
                if (gameManager.IsSelectedDecksFull())
                {
                    // If the deck was full, make the buttons interactable again
                    SetAllButtonsInteractable(true);
                    
                    gameManager.RemoveSelectedDeck(deckIndex);
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
                if (gameManager.GetSelectedDecks().Contains(deckIndex))
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
            gameManager.AddSelectedDeck(deckIndex);

            buttons[deckIndex].SelectButton();

            // If the deck is full, make the buttons non interactable
            if (gameManager.IsSelectedDecksFull())
            {
                SetAllButtonsInteractable(false);
            }
        }

        private void RemoveSelectedDeck(int deckIndex)
        {
            // If the deck was full, make the buttons interactable again
            if (gameManager.IsSelectedDecksFull())
            {
                SetAllButtonsInteractable(true);
            }

            gameManager.RemoveSelectedDeck(deckIndex);
            buttons[deckIndex].DeselectButton();
        }

        private void SetAllButtonsInteractable(bool interactable)
        {
            List<int> selectionnedIndexes = gameManager.GetSelectedDecks();
            for (int i = 0; i < buttons.Count; i++)
            {
                if (!selectionnedIndexes.Contains(i))
                {
                    buttons[i].interactable = interactable;
                }
            }
        }

        private void RemoveSelectedDoubles()
        {
            List<int> indexes = new();
            foreach (int deckIndex in gameManager.GetSelectedDecks())
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

            foreach (CenteredGridLayout grid in grids)
            {
                grid.SetAllParameters(cellSize, gridSpacing, columnNumber);
            }

            foreach (Container container in typeContainers)
            {
                container.SetNameScale(typeNameScale);
                container.SetNameWidth(typeNameWidth);
                container.SetSpacing(typeSpacing);
            }

            foreach (Container container in categoryContainers)
            {
                container.SetNameScale(categoryNameScale);
                container.SetNameWidth(categoryNameWidth);
                container.SetSpacing(categorySpacing);
            }
        }
    }
}
