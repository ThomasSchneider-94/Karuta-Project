using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using Karuta.UIComponent;

namespace Karuta.EditorLayout
{
    [CustomEditor(typeof(LabeledToggle))]
    public class LabeledToggleEditor : ToggleEditor
    {
        [Header("Toggle Label")]
        SerializedProperty labelText;
        SerializedProperty label;

        protected override void OnEnable()
        {
            base.OnEnable();

            labelText = serializedObject.FindProperty("labelText");
            label = serializedObject.FindProperty("label");
        }

        public override void OnInspectorGUI()
        {
            // Affiche les propri�t�s de base du Button
            base.OnInspectorGUI();

            // Rafra�chir l'objet s�rialis�
            serializedObject.Update();

            // Afficher les propri�t�s personnalis�es de VolumeSlider
            EditorGUILayout.PropertyField(labelText);
            EditorGUILayout.PropertyField(label);

            // Appliquer les changements
            serializedObject.ApplyModifiedProperties();
        }
    }
}