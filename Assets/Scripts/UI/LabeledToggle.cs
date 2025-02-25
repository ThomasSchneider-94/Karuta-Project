using UnityEngine;
using UnityEngine.UI;

namespace Karuta.UI
{
    public class LabeledToggle : Toggle
    {
        [Header("Toggle Label")]
        [SerializeField] private Text labelText;
        [SerializeField] private string label;
        public Outline outline;

        public Image Background { get; private set; }
        public Image BackgroundOutline {  get; private set; }

        override protected void Awake()
        {
            base.Awake();

            Background = GetComponent<Image>();
            BackgroundOutline = GetComponent<Image>();
        }


        public void SetLabel(string label)
        {
            this.label = label;
            labelText.text = label;
        }

        public Text GetText()
        {
            return labelText;
        }

        public Image GetBackgound()
        {
            return background;
        }

        public Image GetBackgoundOutline()
        {
            return backgroundOutline;
        }

        /*
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            SetLabel(label);
        }
#endif*/
    }
}