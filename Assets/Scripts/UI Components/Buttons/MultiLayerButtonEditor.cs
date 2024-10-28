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
            // Affiche les propri�t�s de base du Button
            base.OnInspectorGUI();

            // Rafra�chir l'objet s�rialis�
            serializedObject.Update();

            // Afficher les propri�t�s personnalis�es de VolumeSlider
            EditorGUILayout.PropertyField(buttonLayers);

            // Appliquer les changements
            serializedObject.ApplyModifiedProperties();
        }
    }
}