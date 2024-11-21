using UnityEditor;
using UnityEngine;
using Karuta.UIComponent;
using UnityEditor.UI;

namespace Karuta.EditorLayout
{
    [CustomEditor(typeof(NumberButton))]
    public class NumberButtonEditor : ButtonEditor
    {
        SerializedProperty baseColor;

        protected override void OnEnable()
        {
            base.OnEnable();

            baseColor = serializedObject.FindProperty("baseColor");
        }

        public override void OnInspectorGUI()
        {
            // Affiche les propriétés de base du Button
            base.OnInspectorGUI();

            // Rafraîchir l'objet sérialisé
            serializedObject.Update();

            // Afficher les propriétés personnalisées de VolumeSlider
            EditorGUILayout.PropertyField(baseColor);
            
            // Appliquer les changements
            serializedObject.ApplyModifiedProperties();
        }
    }
}