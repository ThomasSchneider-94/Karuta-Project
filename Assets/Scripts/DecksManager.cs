using Karuta.ScriptableObjects;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using static Karuta.ScriptableObjects.JsonObjects;
using System.Xml.Linq;
using UnityEngine.Rendering;

namespace Karuta
{
    /* TODO : 
     * - The download (with loading screen)
     * - Changer la police des boutons des decks
     * - Themes
     * - Video Background
     */


    /* TODO :
         *      - Download and check cover
         *      - Manage disconnexion
         *      - Manage if wrong informations
         */

    public class DecksManager : MonoBehaviour
    {
        public static DecksManager Instance { get; private set; }

        [Header("Selected Decks")]
        [Range(0, 4)]
        [SerializeField] private int selectedDecksMax = 2;
        [SerializeField] private List<int> selectedDecks = new();

        /* The deck list is sorted like this :
         *  - First sorted on the deck category,
         *  - Secondly sorted on the deck type,
         *  - Then the decks are sorted by alphabetical order.
         *  
         *  - Issue with the different category toggle
         *  
         *   The list decksNumbers is the count deck for each category / type couple.
         */
        private readonly List<DeckInfo> deckInfoList = new();
        private readonly List<int> decksCount = new();

        private readonly List<string> categories = new();
        private List<string> types = new();

        public UnityEvent DeckListInitializedEvent { get; } = new UnityEvent();
        public UnityEvent UpdateCategoriesEvent { get; } = new UnityEvent();
        public UnityEvent UpdateDeckListEvent { get; } = new UnityEvent();

        private bool initialized = false;

        private void Awake()
        {
            // Be sure that there is only one instance of DecksManager
            if (Instance == null)
            {
                Instance = this;

                Initialize();

                initialized = true;
            }
            else
            {
                Destroy(gameObject); // Destroy if another DecksManager exist
            }
            DontDestroyOnLoad(gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {
            DeckListInitializedEvent.Invoke();
        }

        private void Initialize()
        {
            // Read the deck categories and deck types
            if (File.Exists(LoadManager.CategoriesFilePath))
            {
                CategoriesAndTypes categoriesAndTypes = JsonUtility.FromJson<CategoriesAndTypes>(File.ReadAllText(LoadManager.CategoriesFilePath));

                foreach (Category category in categoriesAndTypes.categories)
                {
                    categories.Add(category.name);
                }
                types = categoriesAndTypes.types;
            }

            for (int i = 0; i < categories.Count * types.Count; i++)
            {
                decksCount.Add(0);
            }

            LoadDecksInformations();
        }

        public bool IsInitialized()
        {
            return initialized;
        }

        #region Load Deck List
        /// <summary>
        /// Update list of decks information
        /// </summary>
        public void UpdateDeckList()
        {
            deckInfoList.Clear();
            decksCount.Clear();

            List<string> categoriesTMP = new(categories);
            List<string> typesTMP = new(types);

            categories.Clear();
            types.Clear();

            // Read the deck categories and deck types
            if (File.Exists(LoadManager.CategoriesFilePath))
            {
                CategoriesAndTypes categoriesAndTypes = JsonUtility.FromJson<CategoriesAndTypes>(File.ReadAllText(LoadManager.CategoriesFilePath));

                foreach (Category category in categoriesAndTypes.categories)
                {
                    categories.Add(category.name);
                }
                types = categoriesAndTypes.types;
            }

            // Check if the new categories and types are different from the previous ones
            bool different = false;
            if (categories.Count != categoriesTMP.Count || types.Count != typesTMP.Count)
            {
                Debug.Log("Differnts sizes");
                different = true;
            }
            else
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    if (categories[i] != categoriesTMP[i])
                    {
                        Debug.Log("Old category: " + typesTMP[i] + "; New category: " + types[i]);
                        different = true; 
                        break;
                    }
                }
                for (int i = 0; i < types.Count; i++)
                {
                    if (types[i] != typesTMP[i])
                    {
                        Debug.Log("Old type: " + typesTMP[i] + "; New type: " + types[i]);
                        different = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < categories.Count * types.Count; i++)
            {
                decksCount.Add(0);
            }

            LoadDecksInformations();

            // If the categories are the same, only update the deck list. If not update the categories
            if (different)
            {
                Debug.Log("Different");
                UpdateCategoriesEvent.Invoke();
            }
            else
            {
                Debug.Log("Same");
                UpdateDeckListEvent.Invoke();
            }
        }

        /// <summary>
        /// Load the list of decks information
        /// </summary>
        private void LoadDecksInformations()
        {
            // Check if the deck list file exists
            if (File.Exists(LoadManager.DecksFilePath))
            {
                // Read the content of the file
                foreach (JsonDeckInfo jsonDeckInfo in JsonUtility.FromJson<JsonDeckInfoList>(File.ReadAllText(LoadManager.DecksFilePath)).deckInfoList)
                {
                    DeckInfo deckInfo = ScriptableObject.CreateInstance<DeckInfo>();
                    deckInfo.Init(jsonDeckInfo);
                    deckInfoList.Add(deckInfo);
                    decksCount[deckInfo.GetCategory() * types.Count + deckInfo.GetDeckType()]++;
                }
            }
            else
            {
                Debug.LogError("List does not exist " + LoadManager.DecksFilePath);
            }

            DecksInformationsQuickSort(0, deckInfoList.Count - 1);

            Debug.Log(Dump());
        }

        private void DecksInformationsQuickSort(int start, int end)
        {
            if (start >= end || start < 0) { return; }

            int pivot = DecksInformationsPartition(start, end);

            DecksInformationsQuickSort(start, pivot - 1);
            DecksInformationsQuickSort(pivot + 1, end);
        }

        private int DecksInformationsPartition(int start, int end)
        {
            DeckInfo pivot = deckInfoList[end];

            int j = start;
            for (int i = start; i < end; i++)
            {
                if (!deckInfoList[i].IsGreaterThan(pivot))
                {
                    (deckInfoList[i], deckInfoList[j]) = (deckInfoList[j], deckInfoList[i]);
                    j++;
                }
            }
            (deckInfoList[j], deckInfoList[end]) = (deckInfoList[end], deckInfoList[j]);

            return j;
        }

        private string Dump()
        {
            StringBuilder dump = new();

            dump.Append("Counts : [");
            foreach (int count in decksCount)
            {
                dump.Append(count + ", ");
            }
            dump.Append("]\n");

            dump.Append("Decks : [\n");
            foreach (DeckInfo deckInfo in deckInfoList)
            {
                dump.Append("| " + deckInfo.Dump() + "\n");
            }
            dump.Append("]");

            return dump.ToString();
        }
        #endregion Load Deck List

        #region Deck List Getter

        #region Count
        /// <summary>
        /// Get the number of deck of each category / type
        /// </summary>
        /// <returns></returns>
        public List<int> GetDecksCount()
        {
            return decksCount;
        }

        /// <summary>
        /// Get the number of deck of a category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int GetCategoryDecksCount(int category)
        {
            int totalCount = 0;
            for (int i = 0; i < types.Count; i++)
            {
                totalCount += GetTypeDecksCount(category, i);
            }
            return totalCount;
        }

        /// <summary>
        /// Get the number of deck of a type
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetTypeDecksCount(int category, int type)
        {
            return decksCount[category * types.Count + type];
        }
        #endregion Count

        #region Index
        /// <summary>
        /// Get the index of the first deck of the category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int GetCategoryIndex(int category)
        {
            int start = 0;
            for (int i = 0; i < category * types.Count; i++)
            {
                start += decksCount[i];
            }
            return start;
        }

        /// <summary>
        /// Get the index of the first deck of the type
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetTypeIndex(int category, int type)
        {
            int start = GetCategoryIndex(category);
            for (int i = 0; i < type; i++)
            {
                start += decksCount[category * types.Count + i];
            }
            return start;
        }
        #endregion Index

        public List<DeckInfo> GetDeckList()
        {
            return deckInfoList;
        }

        public DeckInfo GetDeck(int i)
        {
            return deckInfoList[i];
        }

        public List<DeckInfo> GetCategoryDeckList(int category)
        {
            return deckInfoList.GetRange(GetCategoryIndex(category), GetCategoryDecksCount(category));
        }
        public List<DeckInfo> GetTypeDeckList(int category, int type)
        {
            return deckInfoList.GetRange(GetTypeIndex(category, type), GetTypeDecksCount(category, type));
        }
        #endregion Deck List Getter

        #region Selected Decks
        public void SetMaxSelectedDeck(int max)
        {
            selectedDecksMax = max;
        }

        public bool IsSelectedDecksFull()
        {
            return selectedDecks.Count >= selectedDecksMax;
        }

        public void AddSelectedDeck(int deckIndex)
        {
            if (selectedDecks.Count < selectedDecksMax)
            {
                selectedDecks.Add(deckIndex);
            }
        }

        public void RemoveSelectedDeck(int deckIndex)
        {
            while (selectedDecks.Contains(deckIndex))
            {
                selectedDecks.Remove(deckIndex);
            }
        }

        public List<int> GetSelectedDecks()
        {
            return selectedDecks;
        }

        public void ClearSelected()
        {
            selectedDecks.Clear();
        }
        #endregion Selected Decks

        #region Categories and Types
        public int GetCategoriesCount()
        {
            return categories.Count;
        }

        public List<string> GetCategories()
        {
            return categories;
        }

        public string GetCategory(int i)
        {
            return categories[i];
        }

        public int GetTypesCount()
        {
            return types.Count;
        }

        public List<string> GetTypes()
        {
            return types;
        }

        public string GetType(int i)
        {
            return types[i];
        }
        #endregion Categories and Types
    }
}