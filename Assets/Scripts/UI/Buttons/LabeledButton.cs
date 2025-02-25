using TMPro;
using UnityEditor;
using UnityEngine;

namespace Karuta.UI.CustomButton
{
    public class LabeledButton : MultiLayerButton
    {
        [Header("Label")]
        [SerializeField] protected TextMeshProUGUI buttonLabelMesh;
        public TextMeshProUGUI ButtonLabelMesh => buttonLabelMesh;
    }
}