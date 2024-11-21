using Karuta.ScriptableObjects;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using static Karuta.ScriptableObjects.JsonObjects;

namespace Karuta
{
    /* TODO : 
     * - Add arrow buttons to UI
     * - Add the selection for the number of deck
     * - The download (with loading screen)
     * - THE GAME
     * - Changer la police des boutons des decks
     * - Themes
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
        private readonly List<int> decksCount = new((int)DeckInfo.DeckCategory.CATEGORY_NB * (int)DeckInfo.DeckType.TYPE_NB);

        public UnityEvent DeckListInitializedEvent { get; } = new UnityEvent();
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
            for (int i = 0; i < (int)DeckInfo.DeckCategory.CATEGORY_NB * (int)DeckInfo.DeckType.TYPE_NB; i++)
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

            for (int i = 0; i < (int)DeckInfo.DeckCategory.CATEGORY_NB * (int)DeckInfo.DeckType.TYPE_NB; i++)
            {
                decksCount.Add(0);
            }

            LoadDecksInformations();

            UpdateDeckListEvent.Invoke();
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
                    decksCount[(int)deckInfo.GetCategory() * (int)DeckInfo.DeckType.TYPE_NB + (int)deckInfo.GetDeckType()]++;
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
        public int GetCategoryCount(DeckInfo.DeckCategory category)
        {
            int totalCount = 0;
            for (int i = 0; i < (int)DeckInfo.DeckType.TYPE_NB; i++)
            {
                totalCount += GetTypeCount(category, (DeckInfo.DeckType)i);
            }
            return totalCount;
        }

        /// <summary>
        /// Get the number of deck of a type
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetTypeCount(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            return decksCount[(int)category * (int)DeckInfo.DeckType.TYPE_NB + (int)type];
        }
        #endregion Count

        #region Index
        /// <summary>
        /// Get the index of the first deck of the category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int GetCategoryIndex(DeckInfo.DeckCategory category)
        {
            int start = 0;
            for (int i = 0; i < (int)category * (int)DeckInfo.DeckType.TYPE_NB; i++)
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
        public int GetTypeIndex(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            int start = GetCategoryIndex(category);
            for (int i = 0; i < (int)type; i++)
            {
                start += decksCount[(int)category * (int)DeckInfo.DeckType.TYPE_NB + i];
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

        public List<DeckInfo> GetCategoryDeckList(DeckInfo.DeckCategory category)
        {
            return deckInfoList.GetRange(GetCategoryIndex(category), GetCategoryCount(category));
        }
        public List<DeckInfo> GetTypeDeckList(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            return deckInfoList.GetRange(GetTypeIndex(category, type), GetTypeCount(category, type));
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
    }
}