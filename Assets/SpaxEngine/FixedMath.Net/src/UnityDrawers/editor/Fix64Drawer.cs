using FixMath.NET;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(Fix64))]
public class Fix64Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var rawProp = property.FindPropertyRelative("m_rawValue");
        Fix64 fpValue = Fix64.BuildFromRawLong(rawProp.longValue);
        fpValue = (Fix64)EditorGUI.FloatField(position, label, (float)fpValue);

        rawProp.longValue = fpValue.m_rawValue;
        EditorUtility.SetDirty(rawProp.serializedObject.targetObject);

        EditorGUI.EndProperty();

    }
}

[CustomPropertyDrawer(typeof(Fix64FloatAttribute))]
public class Fix64FloatDrawer : PropertyDrawer
{
    /*
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2f;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            float floatValue = (float)Fix64.BuildFromRawLong(property.longValue);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = -1;



            float lineHeight = EditorGUIUtility.singleLineHeight;
            floatValue = EditorGUI.FloatField(new Rect(position.x, position.y, position.width, lineHeight), "Float Value", floatValue);
            EditorGUI.LongField(new Rect(position.x, position.y + lineHeight, position.width, lineHeight), "Raw Value", property.longValue);

            EditorGUI.indentLevel = indent;

            if (EditorGUI.EndChangeCheck())
                property.longValue = ((Fix64)floatValue).RawValue;

            EditorGUI.EndProperty();

        }
    */
}

