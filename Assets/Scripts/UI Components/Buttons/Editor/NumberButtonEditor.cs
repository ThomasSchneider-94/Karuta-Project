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
            // Affiche les propri�t�s de base du Button
            base.OnInspectorGUI();

            // Rafra�chir l'objet s�rialis�
            serializedObject.Update();

            // Afficher les propri�t�s personnalis�es de VolumeSlider
            EditorGUILayout.PropertyField(baseColor);
            
            // Appliquer les changements
            serializedObject.ApplyModifiedProperties();
        }
    }
}