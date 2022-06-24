using UnityEditor;
using FixMath.NET;
using UnityEngine;

namespace FixMath.NET.Drawers
{

    [CustomPropertyDrawer(typeof(FVector4))]
    public class FVector4Drawer : PropertyDrawer
    {

        private const int INDENT_OFFSET = 15;
        private const int LABEL_WIDTH = 12;
        private const int LABEL_MARGIN = 1;

        private static GUIContent xLabel = new GUIContent("x");
        private static GUIContent yLabel = new GUIContent("y");
        private static GUIContent zLabel = new GUIContent("z");
        private static GUIContent wLabel = new GUIContent("w");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            position.width /= 4f;
            float indentOffsetLevel = (INDENT_OFFSET) * EditorGUI.indentLevel;
            position.width += indentOffsetLevel;

            EditorGUIUtility.labelWidth = indentOffsetLevel + LABEL_WIDTH;

            SerializedProperty xSerProperty = property.FindPropertyRelative("x");
            position.x -= indentOffsetLevel;
            EditorGUI.PropertyField(position, xSerProperty, xLabel);

            position.x += position.width;

            SerializedProperty ySerProperty = property.FindPropertyRelative("y");
            position.x -= indentOffsetLevel;
            EditorGUI.PropertyField(position, ySerProperty, yLabel);

            position.x += position.width;

            SerializedProperty zSerProperty = property.FindPropertyRelative("z");
            position.x -= indentOffsetLevel;
            EditorGUI.PropertyField(position, zSerProperty, zLabel);

            position.x += position.width;

            SerializedProperty wSerProperty = property.FindPropertyRelative("w");
            position.x -= indentOffsetLevel;
            EditorGUI.PropertyField(position, wSerProperty, wLabel);

            EditorGUI.EndProperty();
        }

    }
}