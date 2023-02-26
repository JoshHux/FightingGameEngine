using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Arr8ObjPoolerWrapper))]
public class Arr8Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var h = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;


        EditorGUI.BeginProperty(position, label, property);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels

        position.height=EditorGUIUtility.singleLineHeight;

        EditorGUI.PropertyField(position, property.FindPropertyRelative("_0"), GUIContent.none, true);
        position.y += h;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_1"), GUIContent.none, true);
        position.y += h;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_2"), GUIContent.none, true);
        position.y += h;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_3"), GUIContent.none, true);
        position.y += h;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_4"), GUIContent.none, true);
        position.y += h;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_5"), GUIContent.none, true);
        position.y += h;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_6"), GUIContent.none, true);
        position.y += h;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_7"), GUIContent.none, true);
        position.y += h;

        EditorGUI.EndProperty();
    }

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
 
 
        float totalHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing)*8;
 
        
 
        return totalHeight;
    }
}
