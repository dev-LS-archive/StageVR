using UnityEditor;

namespace FluidFlow
{
    public class UpdaterCustomInspector
    {
        private SerializedProperty modeProperty;
        private SerializedProperty intervalProperty;

        public UpdaterCustomInspector(SerializedProperty serializedProperty)
        {
            modeProperty = serializedProperty.FindPropertyRelative("UpdateMode");
            intervalProperty = serializedProperty.FindPropertyRelative("FixedUpdateInterval");
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(modeProperty);
            if (!modeProperty.hasMultipleDifferentValues && modeProperty.enumValueIndex == (int)Updater.Mode.FIXED) {
                using (var indented = new EditorGUI.IndentLevelScope(1))
                    EditorGUILayout.PropertyField(intervalProperty);
            }
        }
    }
}