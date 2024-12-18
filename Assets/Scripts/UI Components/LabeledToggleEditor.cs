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
        SerializedProperty outline;

        [Header("Check Mark Background")]
        SerializedProperty backgroundOutline;
        SerializedProperty background;

        protected override void OnEnable()
        {
            base.OnEnable();

            labelText = serializedObject.FindProperty("labelText");
            label = serializedObject.FindProperty("label");
            outline = serializedObject.FindProperty("outline");
            backgroundOutline = serializedObject.FindProperty("backgroundOutline");
            background = serializedObject.FindProperty("background");
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
            EditorGUILayout.PropertyField(outline);
            EditorGUILayout.PropertyField(backgroundOutline);
            EditorGUILayout.PropertyField(background);

            // Appliquer les changements
            serializedObject.ApplyModifiedProperties();
        }
    }
}