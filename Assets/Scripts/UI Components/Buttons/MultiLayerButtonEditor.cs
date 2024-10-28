using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using Karuta.UIComponent;

namespace Karuta.EditorLayout
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
            // Affiche les propriétés de base du Button
            base.OnInspectorGUI();

            // Rafraîchir l'objet sérialisé
            serializedObject.Update();

            // Afficher les propriétés personnalisées de VolumeSlider
            EditorGUILayout.PropertyField(buttonLayers);

            // Appliquer les changements
            serializedObject.ApplyModifiedProperties();
        }
    }
}