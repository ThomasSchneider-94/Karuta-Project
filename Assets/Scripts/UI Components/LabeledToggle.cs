using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Karuta.UIComponent
{
    public class LabeledToggle : Toggle
    {
        [Header("Toggle Label")]
        [SerializeField] private Text labelText;
        [SerializeField] private string label;

        public void SetLabel(string label)
        {
            this.label = label;
            labelText.text = label;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            SetLabel(label);
        }
#endif
    }
}