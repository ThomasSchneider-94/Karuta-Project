using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using Karuta.UI.CustomButton;

namespace Karuta.UI.EditorLayout
{
    [CustomEditor(typeof(MultiLayerButton))]
    public class MultiLayerButtonEditor : ButtonEditor
    {
        [Header("Button Layers")]
        SerializedProperty buttonLayers;

        protected override void OnEnable()
        {
            base.OnEnable();

            buttonLayers = serializedObject.FindProperty("buttonLayers");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(buttonLayers);

            serializedObject.ApplyModifiedProperties();
        }
    }
}