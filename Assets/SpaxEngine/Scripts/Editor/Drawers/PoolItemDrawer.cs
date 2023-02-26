using UnityEngine;
using UnityEditor;

namespace FightingGameEngine.ObjectPooler.Drawers
{

    [CustomPropertyDrawer(typeof(PoolItem))]
    public class PoolItemDrawer : PropertyDrawer
    {
        private const int INDENT_OFFSET = 1;
        private const int LABEL_WIDTH = 75;
        private const int LABEL_MARGIN = 1;


        private static GUIContent objLabel = new GUIContent("projectile");
        private static GUIContent maxLabel = new GUIContent("Max Amount");
        private static GUIContent actLabel = new GUIContent("Active Count");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);
            position.width = 500f;

            position.width /= 2f;
            float indentOffsetLevel = (INDENT_OFFSET) * EditorGUI.indentLevel;
            position.width += indentOffsetLevel;

            EditorGUIUtility.labelWidth = indentOffsetLevel + LABEL_WIDTH;

            SerializedProperty objProp = property.FindPropertyRelative("_pooledObject");
            var objRect = new Rect(position.position, position.size);
            objRect.width = 220f;
            EditorGUI.ObjectField(objRect, objProp, objLabel);

            //objProp.objectReferenceValue = gofield;

            position.x += objRect.width + LABEL_MARGIN;
            SerializedProperty maxProp = property.FindPropertyRelative("_maxAmount");
            var maxRect = new Rect(position.position, position.size);
            maxRect.width = 150f;
            EditorGUI.PropertyField(maxRect, maxProp, maxLabel);

            position.x += maxRect.width + LABEL_MARGIN;
            SerializedProperty actProp = property.FindPropertyRelative("_activeCount");
            var actRect = new Rect(position.position, position.size);
            actRect.width = 150f;
            EditorGUI.PropertyField(actRect, actProp, actLabel);




            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            return totalHeight;
        }
    }
}