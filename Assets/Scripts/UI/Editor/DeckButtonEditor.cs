using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using Karuta.UI.CustomButton;

namespace Karuta.UI.EditorLayout
{
    [CustomEditor(typeof(DeckButton))]
    public class DeckButtonEditor : ColorFadeButtonEditor
    {
        [Header("General")]
        SerializedProperty DeckName;
        SerializedProperty NameWidth;
        SerializedProperty NameSpacing;
        SerializedProperty Count;

        protected override void OnEnable()
        {
            base.OnEnable();

            DeckName = serializedObject.FindProperty("deckName");
            NameWidth = serializedObject.FindProperty("nameWidth");
            NameSpacing = serializedObject.FindProperty("nameSpacing");
            Count = serializedObject.FindProperty("count");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(DeckName);
            EditorGUILayout.PropertyField(NameWidth);
            EditorGUILayout.PropertyField(NameSpacing);
            EditorGUILayout.PropertyField(Count);

            serializedObject.ApplyModifiedProperties();
        }
    }
}