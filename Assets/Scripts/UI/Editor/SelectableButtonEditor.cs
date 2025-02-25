using UnityEditor;
using UnityEngine;
using Karuta.UI.CustomButton;

namespace Karuta.UI.EditorLayout
{
    [CustomEditor(typeof(SelectableButton))]
    public class SelectableButtonEditor : MultiLayerButtonEditor
    {
        [Header("Selection")]
        SerializedProperty isSelected;
        SerializedProperty SelectedColor;

        protected override void OnEnable()
        {
            base.OnEnable();

            isSelected = serializedObject.FindProperty("isSelected");
            SelectedColor = serializedObject.FindProperty("selectedColor");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(isSelected);
            EditorGUILayout.PropertyField(SelectedColor);

            serializedObject.ApplyModifiedProperties();
        }
    }
}