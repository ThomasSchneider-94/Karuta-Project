using UnityEditor;
using UnityEngine;
using Karuta.UI.CustomButton;
using UnityEditor.UI;

namespace Karuta.EditorLayout
{
    [CustomEditor(typeof(NumberButton))]
    public class NumberButtonEditor : ButtonEditor
    {
        SerializedProperty baseColor;
        SerializedProperty selectedColor;
        SerializedProperty text;

        protected override void OnEnable()
        {
            base.OnEnable();

            baseColor = serializedObject.FindProperty("baseColor");
            selectedColor = serializedObject.FindProperty("selectedColor");
            text = serializedObject.FindProperty("text");
        }

        public override void OnInspectorGUI()
        {
            // Affiche les propriétés de base du Button
            base.OnInspectorGUI();

            // Rafraîchir l'objet sérialisé
            serializedObject.Update();

            // Afficher les propriétés personnalisées de VolumeSlider
            EditorGUILayout.PropertyField(baseColor);
            EditorGUILayout.PropertyField(selectedColor);
            EditorGUILayout.PropertyField(text);

            // Appliquer les changements
            serializedObject.ApplyModifiedProperties();
        }
    }
}