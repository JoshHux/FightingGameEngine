using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FightingGameEngine.Data;

namespace FightingGameEngine.UnityInspector
{

    [CustomEditor(typeof(soUniversalStateHolder))]
    public class UniversalStateEdjtor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            soUniversalStateHolder charData = (soUniversalStateHolder)target;
            if (GUILayout.Button("auto-assign state id's"))
            {
                charData.AssignStateID();
            }
        }
    }
}