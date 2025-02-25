using Karuta;
using Karuta.UI.CustomButton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Karuta.Menu
{
    public class DeckNumberSelection : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField, Range(1, 4)] private int startValue;
        [SerializeField] private List<ColorSwapButton> buttons;

        private void Awake()
        {
            SelectDeckNumber(2);
        }

        public void SelectDeckNumber(int value)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].SelectButton(i+1 == value);
            }
            DecksManager.Instance.SetMaxSelectedDeck(value);
        }

    }
}