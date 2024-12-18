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
        [SerializeField] private Outline outline;

        [Header("Check Mark Background")]
        [SerializeField] private Image backgroundOutline;
        [SerializeField] private Image background;

        public void SetLabel(string label)
        {
            this.label = label;
            labelText.text = label;
        }

        public Text GetText()
        {
            return labelText;
        }

        public Outline GetOutline()
        {
            return outline;
        }

        public Image GetBackgound()
        {
            return background;
        }

        public Image GetBackgoundOutline()
        {
            return backgroundOutline;
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