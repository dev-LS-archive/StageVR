using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FluidFlow
{
    [CustomEditor(typeof(FFBrushSO), true)]
    [CanEditMultipleObjects]
    public class FFBrushCustomInspector : Editor
    {
        private SerializedProperty typeProperty;
        private SerializedProperty colorProperty;
        private SerializedProperty dataProperty;
        private SerializedProperty fadeProperty;

        private void OnEnable()
        {
            var brush = serializedObject.FindProperty("Brush");
            typeProperty = brush.FindPropertyRelative("BrushType");
            colorProperty = brush.FindPropertyRelative("Color");
            dataProperty = brush.FindPropertyRelative("Data");
            fadeProperty = brush.FindPropertyRelative("Fade");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            using (var indent = new EditorGUI.IndentLevelScope(1)) {
                EditorGUILayout.PropertyField(typeProperty);
                EditorGUILayout.PropertyField(colorProperty);
                if ((FFBrush.Type)typeProperty.enumValueIndex == FFBrush.Type.FLUID)
                    EditorGUILayout.PropertyField(dataProperty);
                EditorGUILayout.PropertyField(fadeProperty);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}