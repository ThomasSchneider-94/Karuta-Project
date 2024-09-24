using Karuta.ScriptableObjects;
using Karuta.UIComponent;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor.Search;
using UnityEngine;

namespace Karuta.Menu
{
    public class DeckSelection : MonoBehaviour
    {
        private GameManager gameManager;

        [SerializeField] private Transform allButtonsParent;

        [Header("Prefabs")]
        [SerializeField] private CategoryButtonContainer categoryPanel;
        [SerializeField] private GameObject typePanel;


        void Start()
        {
            gameManager = GameManager.Instance;

            CreateDeckButtons();
        }

        #region Create Buttons

        private void CreateDeckButtons()
        {
            // Séparer decks en Category / Type
            List<List<List<DeckInfo>>> decksSorted = new List<List<List<DeckInfo>>>();
            for (int i = 0; i < (int)DeckInfo.DeckCategory.CATEGORY_NB; i++)
            {
                decksSorted.Add(new List<List<DeckInfo>>());
                for (int j = 0; j < (int)DeckInfo.DeckType.TYPE_NB; j++)
                {
                    decksSorted[i].Add(new List<DeckInfo>());
                }
            }

            Debug.Log("Coucou");

            foreach (DeckInfo deck in gameManager.GetDeckList())
            {
                Debug.Log("Selection : " + deck.Dump());

                /*
                 * The decksSorted list works like tihs : [categories * [types * [decks]]]
                 */

                decksSorted[(int)deck.GetCategory()][(int)deck.GetDeckType()].Add(deck);
            }

            CreateDeckButtonsCategory(decksSorted);
        }








        private void CreateDeckButtonsCategory(List<List<List<DeckInfo>>> deckCategories)
        {
            for (int i = 0; i < deckCategories.Count; i++)
            {



                CategoryButtonContainer categoryButtonContainer = Instantiate<CategoryButtonContainer>(categoryPanel, allButtonsParent);
                categoryButtonContainer.SetCategoryName(((DeckInfo.DeckCategory)i).ToString());
            }







            foreach (List<List<DeckInfo>> deckCategory in deckCategories)
            {
                
            }












        }









        #endregion Create Buttons



    }
}
