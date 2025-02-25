using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using Karuta.Objects;
using System;

namespace Karuta
{
    #region Json Objects
    [Serializable]
    public class CategoriesAndTypes
    {
        public List<Category> categories;
        public List<string> types;
    }

    [Serializable]
    public class Category
    {
        public string name;
        public string icon;
    }
    #endregion Json Objects

    /* TODO :
     * - Themes
     * - Upgrade the print for download
     * - Refactor pour s�curiser quand aucune data
     * - Disable wrapping in text
     * - Init visuals during game
     * - GetComponents
     * - Remove Get___()
     * - Make a download screen manager
     */

    /* BUGS : 
     * - KARUTO - Probl�me d'alignement quand on active le t�l�chargement
     * - Bouttons ne semble plus se disable quand tous les decks sont s�lection�s
     * - Le sprite de default est trop bas dans le jeu
     */

    public class DecksManager : MonoBehaviour
    {
        public static DecksManager Instance { get; private set; }

        [Header("Selected Decks")]
        [Range(0, 4)]
        [SerializeField] private int selectedDecksMax = 2;
        [SerializeField] private List<int> selectedDecks = new();

        /* The Deck list is sorted like this :
         *  - First sorted on the Deck Category,
         *  - Secondly sorted on the Deck CardType,
         *  - Then the decks are sorted by alphabetical order.
         *  
         *  - Issue with the different Category toggle
         *  
         *   The list decksNumbers is the count Deck for each Category / CardType couple.
         */
        private readonly List<DeckInfo> deckInfoList = new();
        private readonly List<int> decksCount = new();

        private readonly List<string> categories = new();
        private List<string> types = new();

        public UnityEvent DeckManagerInitializedEvent { get; } = new();
        public UnityEvent UpdateCategoriesAndTypesEvent { get; } = new();
        public UnityEvent UpdateDeckListEvent { get; } = new();

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
            DeckManagerInitializedEvent.Invoke();
        }

        private void Initialize()
        {
            LoadCategoriesAndTypes();

            for (int i = 0; i < categories.Count * types.Count; i++)
            {
                decksCount.Add(0);
            }

            LoadDecksInformation();
        }

        public bool IsInitialized()
        {
            return initialized;
        }

        #region Update Categories, Types and Deck List
        public void UpdateCategoriesAndTypes(CategoriesAndTypes newCategoriesAndTypes)
        {
            bool different = false;
            // Check if the new categories and types are different from the previous ones
            if (categories.Count != newCategoriesAndTypes.categories.Count || types.Count != newCategoriesAndTypes.types.Count)
            {
                Debug.Log("Diffrents sizes");
                different = true;
            }
            else
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    if (categories[i] != newCategoriesAndTypes.categories[i].name)
                    {
                        Debug.Log("Old category: " + categories[i] + "; New category: " + newCategoriesAndTypes.categories[i].name[i]);
                        different = true;
                        break;
                    }
                }
                for (int i = 0; i < types.Count; i++)
                {
                    if (types[i] != newCategoriesAndTypes.types[i])
                    {
                        Debug.Log("Old type: " + types[i] + "; New type: " + newCategoriesAndTypes.types[i]);
                        different = true;
                        break;
                    }
                }
            }

            if (different)
            {
                categories.Clear();
                types.Clear();

                foreach (Category category in newCategoriesAndTypes.categories)
                {
                    categories.Add(category.name);
                }
                types = newCategoriesAndTypes.types;
                UpdateCategoriesAndTypesEvent.Invoke();
            }
        }

        /// <summary>
        /// Update list of decks information
        /// </summary>
        /// 
        public void UpdateDeckList(JsonDeckInfoList jsonDeckList)
        {
            deckInfoList.Clear();
            decksCount.Clear();

            for (int i = 0; i < categories.Count * types.Count; i++)
            {
                decksCount.Add(0);
            }

            // Read the content of the file
            foreach (JsonDeckInfo jsonDeckInfo in jsonDeckList.deckInfoList)
            {
                DeckInfo deckInfo = new(jsonDeckInfo);

                if (deckInfo.Category >= categories.Count && deckInfo.DeckType >= types.Count)
                {
                    Debug.LogWarning("Deck " + deckInfo.DeckName + " category or type is not supported");
                    continue;
                }
                deckInfoList.Add(deckInfo);
                decksCount[deckInfo.Category * types.Count + deckInfo.DeckType]++;
            }

            DecksInformationsQuickSort(0, deckInfoList.Count - 1);

            Debug.Log(Dump());

            UpdateDeckListEvent.Invoke();
        }
        #endregion Update Categories, Types and Deck List

        #region Load Categories, Types and Deck List
        /// <summary>
        /// Load the categories and types
        /// </summary>
        private void LoadCategoriesAndTypes()
        {
            // Read the Deck categories and Deck types
            if (File.Exists(LoadManager.CategoriesFilePath))
            {
                CategoriesAndTypes categoriesAndTypes = JsonUtility.FromJson<CategoriesAndTypes>(File.ReadAllText(LoadManager.CategoriesFilePath));

                foreach (Category category in categoriesAndTypes.categories)
                {
                    categories.Add(category.name);
                }
                types = categoriesAndTypes.types;
            }
        }

        /// <summary>
        /// Load the list of decks information
        /// </summary>
        private void LoadDecksInformation()
        {
            // Check if the Deck list file exists
            if (File.Exists(LoadManager.DecksFilePath))
            {
                // Read the content of the file
                foreach (JsonDeckInfo jsonDeckInfo in JsonUtility.FromJson<JsonDeckInfoList>(File.ReadAllText(LoadManager.DecksFilePath)).deckInfoList)
                {
                    DeckInfo deckInfo = new(jsonDeckInfo);

                    if (deckInfo.Category >= categories.Count && deckInfo.DeckType >= types.Count)
                    {
                        Debug.LogWarning("Deck " + deckInfo.DeckName + " category or type is not supported");
                        continue;
                    }
                    deckInfoList.Add(deckInfo);
                    decksCount[deckInfo.Category * types.Count + deckInfo.DeckType]++;
                }
            }
            else
            {
                Debug.LogWarning("List does not exist " + LoadManager.DecksFilePath);
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

            dump.Append("Counts : [" + string.Join(", ", decksCount) + "]\n");

            dump.Append("Decks : [\n");
            foreach (DeckInfo deckInfo in deckInfoList)
            {
                dump.Append("| " + deckInfo.Dump() + "\n");
            }
            dump.Append("]");

            return dump.ToString();
        }
        #endregion Load Categories, Types and Deck List

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

        #region Deck List Getter
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

        #region Count
        /// <summary>
        /// Get the number of Deck of each Category / CardType
        /// </summary>
        /// <returns></returns>
        public List<int> GetDecksCount()
        {
            return decksCount;
        }

        /// <summary>
        /// Get the number of Deck of a Category
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
        /// Get the number of Deck of a CardType
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
        /// Get the index of the first Deck of the Category
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
        /// Get the index of the first Deck of the CardType
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
        #endregion Deck List Getter

        #region Categories Getter
        public int GetCategoriesCount()
        {
            return categories.Count;
        }

        public List<string> GetCategories()
        {
            return categories;
        }

        public string GetCategoryName(int i)
        {
            return categories[i];
        }
        #endregion Categories Getter

        #region Types Getter
        public int GetTypesCount()
        {
            return types.Count;
        }

        public List<string> GetTypes()
        {
            return types;
        }

        public string GetTypeName(int i)
        {
            return types[i];
        }
        #endregion Types Getter
    }
}