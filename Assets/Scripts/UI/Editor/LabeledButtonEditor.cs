using UnityEditor;
using Karuta.UI.CustomButton;
using UnityEngine;

namespace Karuta.UI.EditorLayout
{
    [CustomEditor(typeof(LabeledButton))]
    public class LabeledButtonEditor : MultiLayerButtonEditor
    {
        [Header("Label")]
        SerializedProperty ButtonLabelMesh;

        protected override void OnEnable()
        {
            base.OnEnable();

            ButtonLabelMesh = serializedObject.FindProperty("buttonLabelMesh");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(ButtonLabelMesh);

            serializedObject.ApplyModifiedProperties();
        }
    }
}