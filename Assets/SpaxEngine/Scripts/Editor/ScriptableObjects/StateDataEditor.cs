using UnityEngine;
using UnityEditor;
using FightingGameEngine.Data;

namespace FightingGameEngine.UnityInspector
{
    [CustomEditor(typeof(soStateData))]
    public class StateDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            soStateData charData = (soStateData)target;
            if (GUILayout.Button("Compile Program"))
            {
                charData.Compile();
            }
        }
    }
}