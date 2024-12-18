using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using Karuta.UIComponent;

namespace Karuta.EditorLayout
{
    [CustomEditor(typeof(MultiLayerButton))]
    public class MultiLayerButtonEditor : ButtonEditor
    {
        SerializedProperty textMesh;

        [Header("Button Layers")]
        SerializedProperty buttonLayers;

        [Header("Disabled")]
        SerializedProperty useDisabledColor;

        protected override void OnEnable()
        {
            base.OnEnable();

            textMesh = serializedObject.FindProperty("textMesh");
            buttonLayers = serializedObject.FindProperty("buttonLayers");
            useDisabledColor = serializedObject.FindProperty("useDisabledColor");
        }

        public override void OnInspectorGUI()
        {
            // Affiche les propriétés de base du Button
            base.OnInspectorGUI();

            // Rafraîchir l'objet sérialisé
            serializedObject.Update();

            // Afficher les propriétés personnalisées de VolumeSlider
            EditorGUILayout.PropertyField(textMesh);
            EditorGUILayout.PropertyField(buttonLayers);
            EditorGUILayout.PropertyField(useDisabledColor);

            // Appliquer les changements
            serializedObject.ApplyModifiedProperties();
        }
    }
}