using UnityEngine;
using UnityEditor;
using FightingGameEngine.Data;

namespace FightingGameEngine.UnityInspector
{
    [CustomEditor(typeof(soCharacterData))]
    public class CharacterDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            soCharacterData charData = (soCharacterData)target;
            if (GUILayout.Button("auto-assign state id's"))
            {
                charData.AssignStateID();
            }
            charData.ApplyEditorPoolItems();
        }
    }
}