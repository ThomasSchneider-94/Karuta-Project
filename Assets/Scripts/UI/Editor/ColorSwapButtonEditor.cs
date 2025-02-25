using UnityEditor;
using Karuta.UI.CustomButton;

namespace Karuta.UI.EditorLayout
{
    [CustomEditor(typeof(ColorSwapButton))]
    public class ColorSwapButtonEditor : SelectableButtonEditor
    {
        SerializedProperty BaseColor;

        protected override void OnEnable()
        {
            base.OnEnable();

            BaseColor = serializedObject.FindProperty("baseColor");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(BaseColor);

            serializedObject.ApplyModifiedProperties();
        }
    }
}