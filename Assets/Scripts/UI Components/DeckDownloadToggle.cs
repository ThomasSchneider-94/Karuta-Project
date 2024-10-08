using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Karuta.UIComponent
{
    public class DeckDownloadToggle : MonoBehaviour
    {
        [Header("Intern Objects")]
        [SerializeField] private Toggle toggle;
        [SerializeField] private Text text;

        public UnityEvent ChangeToggleValue { get; } = new UnityEvent();

        public void OnValueChanged()
        {
            ChangeToggleValue.Invoke();
        }

        public void SetIsOnWithoutNotify(bool value)
        {
            toggle.SetIsOnWithoutNotify(value);
        }

        public void SetDeckName(string value)
        {
            text.text = value;
        }

        public bool IsOn()
        {
            return toggle.isOn;
        }
    }
}