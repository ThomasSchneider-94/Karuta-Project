using UnityEditor;
using UnityEngine;
using Karuta.UIComponent;

namespace Karuta.EditorLayout
{
    [CustomEditor(typeof(SelectionButton))]
    public class DeckSelectionButtonEditor : MultiLayerButtonEditor
    {
        [Header("Name")]
        SerializedProperty deckNameTextMesh;
        SerializedProperty deckName;
        SerializedProperty nameWidth;
        SerializedProperty nameSpacing;

        [Header("Selected Color")]
        SerializedProperty isSelected;
        SerializedProperty selectedColor;

        [Header("Counter")]
        SerializedProperty count;
        SerializedProperty counterImage;
        SerializedProperty counterTextMesh;

        protected override void OnEnable()
        {
            base.OnEnable();

            deckNameTextMesh = serializedObject.FindProperty("deckNameTextMesh");
            deckName = serializedObject.FindProperty("deckName");
            nameWidth = serializedObject.FindProperty("nameWidth");
            nameSpacing = serializedObject.FindProperty("nameSpacing");

            isSelected = serializedObject.FindProperty("isSelected");
            selectedColor = serializedObject.FindProperty("selectedColor");

            count = serializedObject.FindProperty("count");
            counterImage = serializedObject.FindProperty("counterImage");
            counterTextMesh = serializedObject.FindProperty("counterTextMesh");
        }

        public override void OnInspectorGUI()
        {
            // Affiche les propriétés de base du Button
            base.OnInspectorGUI();

            // Rafraîchir l'objet sérialisé
            serializedObject.Update();

            // Afficher les propriétés personnalisées de VolumeSlider
            EditorGUILayout.PropertyField(deckNameTextMesh);
            EditorGUILayout.PropertyField(deckName);
            EditorGUILayout.PropertyField(nameWidth);
            EditorGUILayout.PropertyField(nameSpacing);

            EditorGUILayout.PropertyField(isSelected);
            EditorGUILayout.PropertyField(selectedColor);

            EditorGUILayout.PropertyField(count);
            EditorGUILayout.PropertyField(counterImage);
            EditorGUILayout.PropertyField(counterTextMesh);

            // Appliquer les changements
            serializedObject.ApplyModifiedProperties();
        }
    }
}