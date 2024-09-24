using Karuta.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Karuta.UIComponent {
    public class CategoryButtonContainer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI categoryNameText;
        [SerializeField] private Transform typesButtonsParent;

        public void SetCategoryName(string categoryName) 
        {
            categoryNameText.text = categoryName;
        }

        public void InstanciateTypes(List<List<DeckInfo>> deckTypes)
        {
            for (int i = 0;  i < deckTypes.Count; i++)
            {
                Debug.Log(((DeckInfo.DeckType)i).ToString());
            }
        }
    }
}