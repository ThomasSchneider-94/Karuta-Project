using JetBrains.Annotations;
using Karuta.ScriptableObjects;
using Karuta.UIComponent;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Karuta.Menu
{
    public class DeckSelection : MonoBehaviour
    {
        private GameManager gameManager;

        [SerializeField] private VerticalLayoutGroup allButtonsParent;
        [SerializeField] private float categorySpacing;

        [Header("Category Container Properties")]
        [SerializeField] private float categoryNameScale;
        [SerializeField] private float categoryNameSpacing;
        [SerializeField] private float typeContainerSpacing;

        [Header("Type Container Properties")]
        [SerializeField] private float typeNameScale;
        [SerializeField] private float typeNameSpacing;
        [SerializeField] private float typeNameWidth;

        [Header("Button properties")]
        [Min(1)]
        [SerializeField] private int columnNumber = 3;
        [SerializeField] private Vector2 cellSize = new (100, 100);
        [SerializeField] private Vector2 buttonSpacing = new (10, 20);

        [Header("Prefabs")]
        [SerializeField] private CategoryContainer categoryContainerPrefab;
        [SerializeField] private TypeContainer typeContainerPrefab;
        [SerializeField] private SelectionButton selectionButtonPrefab;

        /* TODO :
         *      - Arrow Buttons
         *      - 
         *      - 
         */

        // Lists
        private readonly List<Transform> categoryContainers = new ();
        private readonly List<Transform> typeContainers = new ();
        private readonly List<SelectionButton> buttons = new ();

        private void Awake()
        {
            gameManager = GameManager.Instance;

            gameManager.UpdateCategoryEvent.AddListener(HideNonUsedContainersAndButtons);
            gameManager.UpdateDeckListEvent.AddListener(UpdateAllContainers);
            Debug.Log("Selection Subscription");
        }

        private void ClearLists()
        {
            for (int i = 0; i < categoryContainers.Count; i++)
            {
                GameObject.Destroy(categoryContainers[i].gameObject);
            }

            categoryContainers.Clear();
            typeContainers.Clear();
            buttons.Clear();
        }

        #region Create Buttons
        /// <summary>
        /// Update containers and create the deck buttons
        /// </summary>
        public void UpdateAllContainers()
        {
            ClearLists();

            CreateAllContainers();

            HideNonUsedContainersAndButtons();
        }

        /// <summary>
        /// Create all containers and create its deck buttons
        /// </summary>
        private void CreateAllContainers()
        {
            allButtonsParent.spacing = categorySpacing;
            CreateCategoryContainers();
        }

        /// <summary>
        /// Create all category containers, its types containers and buttons
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
        /// Create a category container, its types panel and buttons
        /// </summary>
        /// <param name="categoriesDecks"></param>
        /// <param name="category"></param>
        private void CreateCategoryContainer(DeckInfo.DeckCategory category)
        {
            //Debug.Log("S_ " + category.ToString())

            CategoryContainer categoryContainer = GameObject.Instantiate(categoryContainerPrefab);

            categoryContainer.transform.SetParent(allButtonsParent.transform);
            categoryContainers.Add(categoryContainer.transform);

            // Set Parameters
            categoryContainer.SetName(category.ToString());
            categoryContainer.SetNameSpacing(categoryNameSpacing);
            categoryContainer.SetNameScale(categoryNameScale);
            categoryContainer.SetSubContainerSpacing(typeContainerSpacing);

            // Create types containers
            CreateTypeContainers(category);

            // Add the types containers to its childs
            categoryContainer.AddItems(typeContainers.GetRange((int)category * (int)DeckInfo.DeckType.TYPE_NB, (int)DeckInfo.DeckType.TYPE_NB));
        }

        /// <summary>
        /// Create all type containers and its buttons
        /// </summary>
        /// <param name="typesDecks"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        private void CreateTypeContainers(DeckInfo.DeckCategory category)
        {
            for (int i = 0; i < (int)DeckInfo.DeckType.TYPE_NB; i++)
            {
                CreateTypeContainer(category, (DeckInfo.DeckType)i);
            }
        }

        /// <summary>
        /// Create a type container and its buttons
        /// </summary>
        /// <param name="typeDecks"></param>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private void CreateTypeContainer(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            //Debug.Log("S_ " + type.ToString())

            TypeContainer typeContainer = GameObject.Instantiate(typeContainerPrefab);

            typeContainers.Add(typeContainer.transform);

            // Set Parameters
            typeContainer.SetName(type.ToString());
            typeContainer.SetNameSpacing(typeNameSpacing);
            typeContainer.SetNameScale(typeNameScale);
            typeContainer.SetNameWidth(typeNameWidth);

            typeContainer.SetGridParameters(cellSize, buttonSpacing, columnNumber);

            // Create the buttons
            CreateButtons(category, type);

            // Add the buttons to its childs
            typeContainer.AddItems(buttons.GetRange(gameManager.GetTypeIndex(category, type), gameManager.GetTypeCount(category, type)));
        }

        /// <summary>
        /// Create all the buttons of a type
        /// </summary>
        /// <param name="decks"></param>
        /// <param name="category"></param>
        /// <param name="type"></param>
        private void CreateButtons(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            foreach (DeckInfo deck in gameManager.GetTypeDeckList(category, type))
            {
                CreateButton(deck);
            }
        }

        /// <summary>
        /// Create a button and add it to the list
        /// </summary>
        /// <param name="deck"></param>
        /// <param name="category"></param>
        /// <param name="type"></param>
        private void CreateButton(DeckInfo deck)
        {
            SelectionButton button = GameObject.Instantiate(selectionButtonPrefab);

            button.SetDeckName(deck.GetName());
            button.SetThirdLayerSprite(deck.GetCover());
            button.SetInteractable(true);

            buttons.Add(button);
        }
        #endregion Create Buttons

        #region Button Gestion
        private void HideNonUsedContainers()
        {

            for (int i = 0; i < (int)DeckInfo.DeckCategory.CATEGORY_NB; i++)
            {
                categoryContainers[i].gameObject.SetActive(gameManager.GetCategoryCount((DeckInfo.DeckCategory)i) > 0);

                for (int j = 0; j < (int)DeckInfo.DeckType.TYPE_NB; j++)
                {
                    typeContainers[i * (int)DeckInfo.DeckType.TYPE_NB + j].gameObject.SetActive(gameManager.GetTypeCount((DeckInfo.DeckCategory)i, (DeckInfo.DeckType)j) > 0);
                }
            }
















            // Desactivate all containers
            for (int i = 0; i < categoryContainers.Count; i++)
            {
                categoryContainers[i].gameObject.SetActive(false);
                for (int j = 0; j < (int)DeckInfo.DeckType.TYPE_NB; j++)
                {
                    typeContainers[i * (int)DeckInfo.DeckType.TYPE_NB + j].gameObject.SetActive(false);
                }
            }

            // Activate / Desactivate buttons
            List<DeckInfo> deckList = gameManager.GetDeckList();
            for (int i = 0; i < buttons.Count; i++)
            {
                if (IsDeckActive(deckList[i]))
                {
                    buttons[i].gameObject.SetActive(true);

                    // If a button is activated, activate 
                    typeContainers[(int)deckList[i].GetCategory() * (int)DeckInfo.DeckType.TYPE_NB + (int)deckList[i].GetDeckType()].gameObject.SetActive(true);
                    categoryContainers[(int)deckList[i].GetCategory()].gameObject.SetActive(true);
                }
                else
                {
                    buttons[i].gameObject.SetActive(false);
                    buttons[i].SetSelectionned(false);
                    gameManager.RemoveChosenDeck(deckList[i]);
                }
            }
        }

        private bool IsDeckActive(DeckInfo deckInfo)
        {
            return gameManager.IsCategoryActive(deckInfo.GetCategory());
        }
        #endregion Button Gestion




        // Called if changes in code or editor (not called at runtime)
        virtual protected void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            allButtonsParent.spacing = categorySpacing;

            for (int i = 0; i < typeContainers.Count; i++)
            {
                TypeContainer typeContainer = typeContainers[i].GetComponent<TypeContainer>();
                typeContainer.SetNameScale(typeNameScale);
                typeContainer.SetNameSpacing(typeNameSpacing);
                typeContainer.SetNameWidth(typeNameWidth);

                typeContainer.SetGridParameters(cellSize, buttonSpacing, columnNumber);
            }

            for (int i = 0; i < categoryContainers.Count; i++)
            {
                CategoryContainer categoryContainer = categoryContainers[i].GetComponent<CategoryContainer>();

                categoryContainer.SetNameSpacing(categoryNameSpacing);
                categoryContainer.SetNameScale(categoryNameScale);
                categoryContainer.SetSubContainerSpacing(typeContainerSpacing);
            }
        }
    }
}
